using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace leapcert_back.Dtos.Class
{
    public class CreateClassDto
    {
        public int codigo_professor { get; set; }
        public string descricao { get; set; }
        public string? avaliacao { get; set; }
        public DateTime created_at { get; set; }
        public string nome { get; set; }
        public int? genero { get; set; }
        public List<CreateCourseSectionDto> secoes { get; set; } = new();
        public List<CourseLearningPathItemDto> trilha { get; set; } = new();
        public List<CourseForumTopicDto> forum_topicos { get; set; } = new();
        public List<CourseAssessmentItemDto> avaliacoes_itens { get; set; } = new();
        public List<CourseCertificateDto> certificados { get; set; } = new();
        public CourseTeacherContactDto? contato_professor { get; set; }
    }
}
