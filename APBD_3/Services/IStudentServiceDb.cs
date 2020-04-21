using APBD_3.DTOs.Requests;
using APBD_3.DTOs.Responses;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace APBD_3.Services
{
    public interface IStudentServiceDb
    {
        EnrollStudentResponse EnrollStudent(EnrollStudentRequest request);
        PromoteStudentsResponse PromoteStudents(PromoteStudentRequest request);

        bool CheckIndex(string index);

        void logRequest(string request);
    }
}
