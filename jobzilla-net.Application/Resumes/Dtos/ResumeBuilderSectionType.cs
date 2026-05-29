namespace jobzilla_net.Application.Resumes.Dtos;

/// <summary>
/// Builder-level section types. Unknown/custom sections use SectionType.Custom.
/// </summary>
[System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.JsonStringEnumConverter))]
public enum ResumeBuilderSectionType
{
    PersonalInfo,
    Summary,
    Experience,
    Education,
    Skills,
    Certifications,
    Projects,
    References,
    Awards,
    Languages,
    Publications,
    Volunteer,
    Achievements,
    Custom
}
