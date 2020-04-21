using APBD_3.DTOs.Requests;
using APBD_3.DTOs.Responses;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace APBD_3.Services
{
    public class SqlServerStudentDbService: IStudentServiceDb
    {
        String CONNECTION_STRING = @"Data Source=LAPTOP-11FAC326\SQLEXPRESS;Initial Catalog=s19047;Integrated Security=True";

        public bool CheckIndex(string index)
        {
           
            try
            {
                using (var client = new SqlConnection(CONNECTION_STRING))
                using (var command = new SqlCommand())
                {
                    client.Open();
                    command.Connection = client;
                    command.CommandText = "SELECT IndexNumber FROM Student where IndexNumber = @Index;";
                    
                    command.Parameters.AddWithValue("Index", index);

                    var reader = command.ExecuteReader();
                    return reader.Read() ? true : false;
                   
                }
            }catch(Exception e)
            {
                 
                return false;
            }
        }

        public EnrollStudentResponse EnrollStudent(EnrollStudentRequest request)
        {
            
            var result = new EnrollStudentResponse();
            using (var client = new SqlConnection(CONNECTION_STRING))
            using (var command = new SqlCommand())
            {
                client.Open();
                var tran = client.BeginTransaction();

                command.Connection = client;
                command.Transaction = tran;
                command.CommandText = "SELECT IdStudy FROM Studies where Name = @Name;";
                command.Parameters.AddWithValue("Name", request.Studies);
           
                
                try
                {
                    var CheckStudyReader = command.ExecuteReader();

                    //check if studies exist else rollback + 404
                    if(!CheckStudyReader.Read())
                    {
                        //tran.Rollback();
                        //studies does not exist
                        return null;
                    }
                    int idStudy = int.Parse(CheckStudyReader["IdStudy"].ToString());
                    // Note that the assignment asks to search for values where semester = 1; However, our tables have 
                    // years as semesters so for example 2020 , for that reason I just assumed 2020 as the default semester
                    //instead of 1
                    CheckStudyReader.Close();

                    int enrollId = 0;
                    command.Parameters.Clear();
                    command.CommandText = "SELECT IdEnrollment FROM Enrollment  where Semester = 1 AND IdStudy=@IdStudy";
                    command.Parameters.AddWithValue("IdStudy", idStudy);

                    var CheckEnrollmentReader = command.ExecuteReader();
                    
                    //check if enrollment already exists , else insert one 

                    if (!CheckEnrollmentReader.Read())
                    {
                        CheckEnrollmentReader.Close();
                        command.CommandText = "Select Top 1 IdEnrollment as id from Enrollment Order By IdEnrollment DESC";
                        var GetEnrollmentIdReader = command.ExecuteReader();
                        GetEnrollmentIdReader.Read();
                        enrollId = int.Parse(GetEnrollmentIdReader["id"].ToString());
                        GetEnrollmentIdReader.Close();
                        command.Parameters.Clear();
                        command.CommandText = "insert into Enrollment(IdEnrollment,IdStudy, Semester, StartDate) values (@enroll, @IdStudy, @Semester, @Date)";
                        command.Parameters.AddWithValue("enroll", ++enrollId);
                        command.Parameters.AddWithValue("IdStudy", idStudy);
                        command.Parameters.AddWithValue("Semester", 1);
                        command.Parameters.AddWithValue("Date", DateTime.Parse(DateTime.Now.ToString("yyyy'-'MM'-'dd")) );
                     
                       
                    }
                    else
                    {
                        enrollId = int.Parse(CheckEnrollmentReader["IdEnrollment"].ToString());
                        CheckEnrollmentReader.Close();
                    }
                    
                    //check if index number was assigned to any other student , if not insert student 
                    command.Parameters.Clear();
                    command.CommandText = "Select count(1) as studentCount from Student where IndexNumber = @index";
                    command.Parameters.AddWithValue("index", request.IndexNumber);

                    var CheckIndexReader = command.ExecuteReader();
                    if (CheckIndexReader.Read() && int.Parse(CheckIndexReader["studentCount"].ToString()) > 0)
                    {
                        tran.Rollback();
                        return null;
                    }
                    CheckIndexReader.Close();
                    command.Parameters.Clear();
                    command.CommandText = "INSERT INTO Student(IndexNumber, FirstName, LastName, BirthDate, IdEnrollment) VALUES (@index,@FirstName,@LastName,@Birthdate,@enrollId)";
                    command.Parameters.AddWithValue("index", request.IndexNumber);
                    command.Parameters.AddWithValue("FirstName", request.FirstName);
                    command.Parameters.AddWithValue("LastName", request.LastName);
                    command.Parameters.AddWithValue("BirthDate", request.BirthDate);
                    command.Parameters.AddWithValue("enrollId", enrollId);
                    command.ExecuteNonQuery();

                    result.IdEnrollment = enrollId;
                    result.IndexNumber = request.IndexNumber;
                    result.Semester = 1;
                    tran.Commit();
                    client.Close();
                }
                catch (Exception e)
                {
                    tran.Rollback();
                    return null;
                }

                return result;
            }

        }

        public void logRequest(string request)
        {
            string path = @"C:\Users\progy\Desktop\4th_sem_pjatk\APBD\Tutorial_6\APBD_6\log.txt";

            using (StreamWriter sw = File.AppendText(path))
            {
                sw.Write(request);
            }

        }

        public PromoteStudentsResponse PromoteStudents(PromoteStudentRequest request)
        {
            String test = "0 ";
            var result = new PromoteStudentsResponse();
            //check if Enrollment exists
            using (var client = new SqlConnection(CONNECTION_STRING))
            using (var command = new SqlCommand("PromoteStudents", client) { CommandType = System.Data.CommandType.StoredProcedure })
            {
                test += "1 ";
                client.Open();
                var tran = client.BeginTransaction();
                command.Transaction = tran;
                command.Parameters.AddWithValue("Semester", request.Semester);
                command.Parameters.AddWithValue("StudiesName", request.Studies);
                var reader = command.ExecuteReader();
                try
                {
                    
                    test += "2 ";
                    //create response
                    if (reader.Read())
                    {
                        result.IdEnrollment = int.Parse(reader["IdEnrollment"].ToString());
                        result.IdStudy = int.Parse(reader["IdStudy"].ToString());
                        result.Semester = int.Parse(reader["Semester"].ToString());
                        result.StartDate = DateTime.Parse(reader["StartDate"].ToString());
                    }
                    test += "3 ";
                    tran.Commit();
                    client.Close();
                }catch(Exception e)
                {
                    test += "Roll " + e;
                    reader.Close();
                    tran.Rollback();
                    return null;
                }

             
            }
            test += "end ";
            return result;
        }
    }
}
