namespace jobzilla_net.Application.Resumes.Dtos;

/// <summary>
/// Builder-level section types. Unknown/custom sections use SectionType.Custom.
/// </summary>
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
