namespace jobzilla_net.Application.Resumes.Dtos;

[System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.JsonStringEnumConverter))]
public enum ResumeSectionType
{
    PersonalInfo,
    Summary,
    Experience,
    Education,
    Skills,
    Certifications,
    Projects,
    Languages,
    Achievements,
    Awards,
    Volunteer,
    Publications,
    SocialLinks,
    Unknown
}
