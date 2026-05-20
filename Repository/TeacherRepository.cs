using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using leapcert_back.Context;
using leapcert_back.Dtos.Class;
using leapcert_back.Dtos.Users;
using leapcert_back.Interfaces;
using leapcert_back.Mappers;
using leapcert_back.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static leapcert_back.Responses.ResponseFactory;

namespace leapcert_back.Repository
{
    public class TeacherRepository : ITeacherRepository
    {
        private ApplicationDbContext _context;
        private readonly IMinIoRepository _minioRepository;

        public TeacherRepository(ApplicationDbContext context, IMinIoRepository minioRepository)
        {
            _context = context;
            _minioRepository = minioRepository;
        }

        public async Task<IResponses> GetAllClasses(int id)
        {
            ICollection<UserClass> course = await _context.tb_usuario_curso
                .Include(uc => uc.ClassJoin)
                .Include(uc => uc.ClassJoin)
                .Where(uc => uc.codigo_usuario == id)
                .ToListAsync();

            if (course == null)
                return new ErrorResponse(false, 400, "Nenhum curso encontrado");

            var mappedUserClasses = course.Select(uc => uc.ToReadAllClassesDto());

            return new SuccessResponse<IEnumerable<ReadAllClassesDto>>(true, 200, "Cursos encontrados com sucesso", mappedUserClasses);
        }

        public async Task<IResponses> CreateClass([FromBody] CreateClassDto dto)
        {
            if (dto == null)
                return new ErrorResponse(false, 400, "Informações do curso não podem ser nulas");

            if (dto.codigo_professor < 1)
                return new ErrorResponse(false, 400, "Código do professor inválido. Faça login novamente.");

            var professorExists = await _context.Usuario.AnyAsync(u => u.codigo == dto.codigo_professor);
            if (!professorExists)
                return new ErrorResponse(false, 400, "Professor não encontrado no sistema. Faça login novamente.");

            if (dto.genero is not int gen || gen < 1)
                return new ErrorResponse(false, 400, "Selecione uma categoria válida.");

            var genderExists = await _context.tb_genero.AnyAsync(g => g.codigo == gen);
            if (!genderExists)
                return new ErrorResponse(false, 400, "Categoria inválida.");

            var sections = NormalizeSections(dto.secoes);
            if (sections.Count == 0)
                return new ErrorResponse(false, 400, "Adicione pelo menos uma seção ao curso.");

            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var classMapped = dto.ToCreateClassDto();

                await _context.tb_curso.AddAsync(classMapped);
                await _context.SaveChangesAsync();

                if (classMapped.codigo == 0)
                {
                    await transaction.RollbackAsync();
                    return new ErrorResponse(false, 500, "Erro interno: código do curso não foi gerado.");
                }

                var minioFolder = await _minioRepository.CreateFolder("/", dto.nome);
                if (!minioFolder.Flag)
                {
                    await transaction.RollbackAsync();
                    return minioFolder;
                }

                UserClass newUserClass = new UserClass()
                {
                    codigo_curso = classMapped.codigo,
                    codigo_usuario = dto.codigo_professor,
                    data_matricula = DateTime.UtcNow
                };

                ClassPath newClassPath = new ClassPath()
                {
                    codigo_curso = classMapped.codigo,
                    path = dto.nome + "/",
                };

                await _context.tb_usuario_curso.AddAsync(newUserClass);
                await _context.tb_curso_path.AddAsync(newClassPath);
                await _context.tb_curso_secao.AddRangeAsync(sections.Select((section, index) => new CourseSection
                {
                    codigo_curso = classMapped.codigo,
                    titulo = section.titulo,
                    conteudo = section.conteudo,
                    ordem = index + 1,
                    created_at = DateTime.UtcNow,
                    updated_at = DateTime.UtcNow,
                }));
                await AddCourseTopicsAsync(classMapped.codigo, dto);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return new SuccessResponse<CreateClassResultDto>(true, 200, "Curso criado com sucesso", new CreateClassResultDto
                {
                    codigo_curso = classMapped.codigo,
                    nome = classMapped.nome,
                    path = newClassPath.path,
                });
            }
            catch (Exception e)
            {
                await transaction.RollbackAsync();
                return new ErrorResponse(false, 500, $"Erro ao criar curso: {e.Message}");
            }
        }

        private static List<CreateCourseSectionDto> NormalizeSections(IEnumerable<CreateCourseSectionDto>? sections)
        {
            return sections?
                .Select((section, index) => new CreateCourseSectionDto
                {
                    titulo = section.titulo?.Trim() ?? string.Empty,
                    conteudo = section.conteudo?.Trim() ?? string.Empty,
                    ordem = section.ordem > 0 ? section.ordem : index + 1,
                })
                .Where(section => !string.IsNullOrWhiteSpace(section.titulo) && !string.IsNullOrWhiteSpace(section.conteudo))
                .OrderBy(section => section.ordem)
                .ToList()
                ?? new List<CreateCourseSectionDto>();
        }

        private async Task AddCourseTopicsAsync(int courseId, CreateClassDto dto)
        {
            var now = DateTime.UtcNow;
            await _context.tb_curso_trilha.AddRangeAsync(NormalizeLearningPath(dto.trilha).Select((item, index) => new CourseLearningPathItem
            {
                codigo_curso = courseId,
                titulo = item.titulo,
                tipo = item.tipo,
                concluido_padrao = item.concluido_padrao,
                arquivo_nome = item.arquivo_nome,
                arquivo_path = item.arquivo_path,
                arquivo_tipo = item.arquivo_tipo,
                ordem = index + 1,
                created_at = now,
                updated_at = now,
            }));

            await _context.tb_curso_forum_topico.AddRangeAsync(NormalizeForumTopics(dto.forum_topicos).Select((topic, index) => new CourseForumTopic
            {
                codigo_curso = courseId,
                autor = topic.autor,
                titulo = topic.titulo,
                resumo = topic.resumo,
                ordem = index + 1,
                created_at = now,
                updated_at = now,
            }));

            await _context.tb_curso_avaliacao_item.AddRangeAsync(NormalizeAssessmentItems(dto.avaliacoes_itens).Select((item, index) => new CourseAssessmentItem
            {
                codigo_curso = courseId,
                titulo = item.titulo,
                tipo = item.tipo,
                quantidade_questoes = item.quantidade_questoes,
                duracao = item.duracao,
                prazo = item.prazo,
                ordem = index + 1,
                created_at = now,
                updated_at = now,
            }));

            await _context.tb_curso_certificado.AddRangeAsync(NormalizeCertificates(dto.certificados).Select((certificate, index) => new CourseCertificate
            {
                codigo_curso = courseId,
                titulo = certificate.titulo,
                descricao = certificate.descricao,
                status = certificate.status,
                progresso_padrao = certificate.progresso_padrao,
                disponivel_padrao = certificate.disponivel_padrao,
                ordem = index + 1,
                created_at = now,
                updated_at = now,
            }));

            var contact = NormalizeTeacherContact(dto.contato_professor);
            if (contact != null)
            {
                await _context.tb_curso_professor_contato.AddAsync(new CourseTeacherContact
                {
                    codigo_curso = courseId,
                    nome_professor = contact.nome_professor,
                    subtitulo = contact.subtitulo,
                    mensagem_orientacao = contact.mensagem_orientacao,
                    created_at = now,
                    updated_at = now,
                });
            }
        }

        private static List<CourseLearningPathItemDto> NormalizeLearningPath(IEnumerable<CourseLearningPathItemDto>? items)
        {
            return items?
                .Select((item, index) => new CourseLearningPathItemDto
                {
                    titulo = item.titulo?.Trim() ?? string.Empty,
                    tipo = NormalizeType(item.tipo, "reading"),
                    concluido_padrao = item.concluido_padrao,
                    arquivo_nome = NormalizeNullable(item.arquivo_nome),
                    arquivo_path = NormalizeNullable(item.arquivo_path),
                    arquivo_tipo = NormalizeNullable(item.arquivo_tipo),
                    ordem = item.ordem > 0 ? item.ordem : index + 1,
                })
                .Where(item => !string.IsNullOrWhiteSpace(item.titulo))
                .OrderBy(item => item.ordem)
                .ToList()
                ?? new List<CourseLearningPathItemDto>();
        }

        private static List<CourseForumTopicDto> NormalizeForumTopics(IEnumerable<CourseForumTopicDto>? topics)
        {
            return topics?
                .Select((topic, index) => new CourseForumTopicDto
                {
                    autor = topic.autor?.Trim() ?? string.Empty,
                    titulo = topic.titulo?.Trim() ?? string.Empty,
                    resumo = topic.resumo?.Trim() ?? string.Empty,
                    ordem = topic.ordem > 0 ? topic.ordem : index + 1,
                })
                .Where(topic => !string.IsNullOrWhiteSpace(topic.titulo) && !string.IsNullOrWhiteSpace(topic.resumo))
                .OrderBy(topic => topic.ordem)
                .ToList()
                ?? new List<CourseForumTopicDto>();
        }

        private static List<CourseAssessmentItemDto> NormalizeAssessmentItems(IEnumerable<CourseAssessmentItemDto>? items)
        {
            return items?
                .Select((item, index) => new CourseAssessmentItemDto
                {
                    titulo = item.titulo?.Trim() ?? string.Empty,
                    tipo = NormalizeType(item.tipo, "activity"),
                    quantidade_questoes = item.quantidade_questoes is > 0 ? item.quantidade_questoes : null,
                    duracao = string.IsNullOrWhiteSpace(item.duracao) ? null : item.duracao.Trim(),
                    prazo = item.prazo,
                    ordem = item.ordem > 0 ? item.ordem : index + 1,
                })
                .Where(item => !string.IsNullOrWhiteSpace(item.titulo))
                .OrderBy(item => item.ordem)
                .ToList()
                ?? new List<CourseAssessmentItemDto>();
        }

        private static List<CourseCertificateDto> NormalizeCertificates(IEnumerable<CourseCertificateDto>? certificates)
        {
            return certificates?
                .Select((certificate, index) => new CourseCertificateDto
                {
                    titulo = certificate.titulo?.Trim() ?? string.Empty,
                    descricao = certificate.descricao?.Trim() ?? string.Empty,
                    status = certificate.status?.Trim() ?? string.Empty,
                    progresso_padrao = Math.Clamp(certificate.progresso_padrao, 0, 100),
                    disponivel_padrao = certificate.disponivel_padrao,
                    ordem = certificate.ordem > 0 ? certificate.ordem : index + 1,
                })
                .Where(certificate => !string.IsNullOrWhiteSpace(certificate.titulo) && !string.IsNullOrWhiteSpace(certificate.descricao))
                .OrderBy(certificate => certificate.ordem)
                .ToList()
                ?? new List<CourseCertificateDto>();
        }

        private static CourseTeacherContactDto? NormalizeTeacherContact(CourseTeacherContactDto? contact)
        {
            if (contact == null)
            {
                return null;
            }

            var name = contact.nome_professor?.Trim() ?? string.Empty;
            var subtitle = string.IsNullOrWhiteSpace(contact.subtitulo) ? "Professor do curso" : contact.subtitulo.Trim();
            var message = string.IsNullOrWhiteSpace(contact.mensagem_orientacao)
                ? "Envie uma mensagem diretamente para o professor do curso:"
                : contact.mensagem_orientacao.Trim();

            return string.IsNullOrWhiteSpace(name)
                ? null
                : new CourseTeacherContactDto
                {
                    nome_professor = name,
                    subtitulo = subtitle,
                    mensagem_orientacao = message,
                };
        }

        private static string NormalizeType(string? value, string fallback)
        {
            var type = value?.Trim().ToLowerInvariant();
            return string.IsNullOrWhiteSpace(type) ? fallback : type;
        }

        private static string? NormalizeNullable(string? value)
        {
            var trimmed = value?.Trim();
            return string.IsNullOrWhiteSpace(trimmed) ? null : trimmed;
        }
    }
}
