using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace TestDeserialize
{
    class Program
    {
        static void Main(string[] args)
        {
            var json = "{\"fullName\":\"Young\",\"professionalTitle\":\"Value\",\"email\":\"asda@asd.com\",\"phone\":\"Inside\",\"location\":\"Cream\",\"website\":null,\"linkedInUrl\":null,\"gitHubUrl\":null,\"summary\":null,\"sections\":[{\"id\":\"experience_1780058824185\",\"sectionType\":\"Experience\",\"title\":\"Work Experience\",\"displayOrder\":0,\"isVisible\":true,\"items\":[{\"id\":\"item_1780058832120\",\"displayOrder\":0,\"fields\":[{\"key\":\"JobTitle\",\"label\":\"Job Title\",\"value\":\"Testing\",\"fieldType\":\"text\"},{\"key\":\"CompanyName\",\"label\":\"Company Name\",\"value\":\"testing\",\"fieldType\":\"text\"},{\"key\":\"Location\",\"label\":\"Location\",\"value\":\"\",\"fieldType\":\"text\"},{\"key\":\"StartDate\",\"label\":\"Start Date\",\"value\":\"\",\"fieldType\":\"date\"},{\"key\":\"EndDate\",\"label\":\"End Date\",\"value\":\"\",\"fieldType\":\"date\"},{\"key\":\"Current\",\"label\":\"I currently work here\",\"value\":\"\",\"fieldType\":\"checkbox\"},{\"key\":\"Description\",\"label\":\"Description\",\"value\":\"\",\"fieldType\":\"textarea\"}]}]}]}";
            
            var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
            
            try
            {
                // We need the actual classes from the project to test perfectly, but we can just run this inside a scratch C# project or directly as a CSX script if we have dotnet script.
                // Or I can just write a quick console app that references the Application project.
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
    }
}
