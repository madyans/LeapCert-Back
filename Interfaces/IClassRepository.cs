namespace leapcert_back.Interfaces;

public interface IClassRepository
{
    Task<IResponses> GetAllAsync(int? requestingUserId = null);
    Task<IResponses> GetByIdAsync(int id, int requestingUserId);
    Task<IResponses> GetStudentCoursesAsync(int requestingUserId);
    Task<IResponses> GetStudentProgressAlertsAsync(int requestingUserId);
    Task<IResponses> MarkStudentProgressAlertSeenAsync(int alertId, int requestingUserId);
    Task<IResponses> ConnectToCourseAsync(int courseId, int requestingUserId);
    Task<IResponses> CompleteLearningPathItemAsync(int courseId, int itemId, int requestingUserId);
    Task<IResponses> UncompleteLearningPathItemAsync(int courseId, int itemId, int requestingUserId);
    Task<IResponses> UpsertCourseRatingAsync(int courseId, int requestingUserId, Dtos.Class.UpsertCourseRatingDto dto);
    Task<IResponses> UpdateCourseTopicsAsync(int courseId, int requestingUserId, Dtos.Class.CourseTopicsDto dto);
    Task<IResponses> CreateCourseForumTopicAsync(int courseId, int requestingUserId, Dtos.Class.CreateCourseForumTopicDto dto);
    Task<IResponses> CreateCourseNoteAsync(int courseId, int requestingUserId, Dtos.Class.UpsertCourseUserNoteDto dto);
    Task<IResponses> UpdateCourseNoteAsync(int courseId, int noteId, int requestingUserId, Dtos.Class.UpsertCourseUserNoteDto dto);
    Task<IResponses> DeleteCourseNoteAsync(int courseId, int noteId, int requestingUserId);
    Task<IResponses> GetTeacherByClass(int id);
}
