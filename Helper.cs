﻿using System.Data;
using System.Reflection;
using System.Text;
using System.Text.Json;

namespace WebAPICodeGenerator
{
    public static class Helper
    {

        public static string GetTablePrefix(this string tableName)
        {
            var find = tableName.IndexOf("_");
            return tableName[..(find + 1)];
        }

        public static void GenerateControllerFolder(string parentPath, string tabbleName)
        {
            Directory.CreateDirectory(parentPath + "/" + tabbleName.SnakeToCamelCase().CamelToPascalCase());
        }

        public static List<T> ConvertDataTable<T>(DataTable dt)
        {
            List<T> data = new List<T>();
            foreach (DataRow row in dt.Rows)
            {
                T item = GetItem<T>(row);
                data.Add(item);
            }
            return data;
        }
        private static T GetItem<T>(DataRow dr)
        {
            Type temp = typeof(T);
            T obj = Activator.CreateInstance<T>();

            foreach (DataColumn column in dr.Table.Columns)
            {
                foreach (PropertyInfo pro in temp.GetProperties())
                {
                    if (pro.Name == column.ColumnName)
                        pro.SetValue(obj, dr[column.ColumnName], null);
                    else
                        continue;
                }
            }
            return obj;
        }

        public static string SnakeToCamelCase(this string name)
        {
            try
            {
                var parts = name.Split('_');
                if (parts.Length == 1)
                {
                    return JsonNamingPolicy.CamelCase.ConvertName(name);
                }
                else if (parts.Length > 1)
                {
                    parts[0] = parts[0].ToLower();
                    for (var i = 1; i < parts.Length; i++)
                    {
                        string lower = parts[i].ToLower();
                        parts[i] = lower[..1].ToUpper() + lower[1..];
                    }
                }
                return parts.Aggregate("", (acc, x) => acc + x);
            }
            catch (Exception ex)
            {
                return name;
            }

        }

        public static string CamelToPascalCase(this string name)
        {
            var theFirstLetter = name[..1].ToUpper();
            var theRemainString = name[1..];
            return theFirstLetter + theRemainString;
        }

        public static string GenerateIRepositoryClass(string SNAKE_NAME, string PascalName)
        {
            StringBuilder sb = new();

            sb.AppendLine("using GYM_BE.Core.Generic;");
            sb.AppendLine("using GYM_BE.DTO;");
            sb.AppendLine("using GYM_BE.Entities;");
            sb.AppendLine("");
            sb.AppendLine(string.Format("namespace GYM_BE.All.{0}", PascalName));
            sb.AppendLine("{");
            sb.AppendLine(string.Format("    public interface I{0}Repository: IGenericRepository<{1}, {0}DTO>", PascalName, SNAKE_NAME));
            sb.AppendLine("    {");
            sb.AppendLine("        Task<FormatedResponse> QueryList(PaginationDTO pagination);");
            sb.AppendLine("    }");
            sb.AppendLine("}");

            return sb.ToString();
        }

        public static string GenerateRepositoryClass(string SNAKE_NAME, string PascalName)
        {
            StringBuilder sb = new();

            sb.AppendLine("using GYM_BE.Core.Dto;");
            sb.AppendLine("using GYM_BE.Core.Generic;");
            sb.AppendLine("using GYM_BE.DTO;");
            sb.AppendLine("using GYM_BE.Entities;");
            sb.AppendLine("");
            sb.AppendLine(string.Format("namespace GYM_BE.All.{0}", PascalName));
            sb.AppendLine("{");
            sb.AppendLine(string.Format("    public class {0}Repository : I{0}Repository", PascalName));
            sb.AppendLine("    {");
            sb.AppendLine("        private readonly FullDbContext _dbContext;");
            sb.AppendLine(string.Format("       private readonly GenericRepository<{0}, {1}DTO> _genericRepository;", SNAKE_NAME, PascalName));
            sb.AppendLine("");
            sb.AppendLine(string.Format("        public {0}Repository(FullDbContext context)", PascalName));
            sb.AppendLine("        {");
            sb.AppendLine("            _dbContext = context;");
            sb.AppendLine(string.Format("            _genericRepository = new GenericRepository<{0}, {1}DTO>(_dbContext);", SNAKE_NAME, PascalName));
            sb.AppendLine("        }");
            sb.AppendLine("");
            sb.AppendLine(string.Format("        public async Task<FormatedResponse> QueryList(PaginationDTO pagination)"));
            sb.AppendLine("        {");
            sb.AppendLine(string.Format("            var joined = from p in _dbContext.{0}s.AsNoTracking()", PascalName));
            sb.AppendLine("                                       //tuy chinh");
            sb.AppendLine(string.Format("                       select new {0}DTO", PascalName));
            sb.AppendLine("                        {");
            sb.AppendLine("                            Id = p.ID,");
            sb.AppendLine("                        };");
            sb.AppendLine("         var respose = await _genericRepository.PagingQueryList(joined, pagination);");
            sb.AppendLine("         return new FormatedResponse");
            sb.AppendLine("         {");
            sb.AppendLine("             InnerBody = respose,");
            sb.AppendLine("         };");
            sb.AppendLine("        }");
            sb.AppendLine("");
            sb.AppendLine("        public async Task<FormatedResponse> GetById(long id)");
            sb.AppendLine("        {");
            sb.AppendLine("            var res = await _genericRepository.GetById(id);");
            sb.AppendLine("            if (res.InnerBody != null)");
            sb.AppendLine("            {");
            sb.AppendLine("                var response = res.InnerBody;");
            sb.AppendLine(string.Format("                var list = new List<{0}>", SNAKE_NAME));
            sb.AppendLine("                    {");
            sb.AppendLine(string.Format("                        ({0})response", SNAKE_NAME));
            sb.AppendLine("                    };");
            sb.AppendLine("                var joined = (from l in list");
            sb.AppendLine("                              // JOIN OTHER ENTITIES BASED ON THE BUSINESS");
            sb.AppendLine(string.Format("                              select new {0}DTO", PascalName));
            sb.AppendLine("                              {");
            sb.AppendLine("                                  Id = l.ID");
            sb.AppendLine("                              }).FirstOrDefault();");
            sb.AppendLine("");
            sb.AppendLine("                return new FormatedResponse() { InnerBody = joined };");
            sb.AppendLine("            }");
            sb.AppendLine("            else");
            sb.AppendLine("            {");
            sb.AppendLine(@"                return new FormatedResponse() { MessageCode = ""ENTITY_NOT_FOUND"", ErrorType = EnumErrorType.CATCHABLE, StatusCode = EnumStatusCode.StatusCode400 };");
            sb.AppendLine("            }");
            sb.AppendLine("        }");
            sb.AppendLine("");

            sb.AppendLine(string.Format("        public async Task<FormatedResponse> Create({0}DTO dto, string sid)", PascalName));
            sb.AppendLine("        {");
            sb.AppendLine("            var response = await _genericRepository.Create(dto, \"root\");");
            sb.AppendLine("            return response;");
            sb.AppendLine("        }");
            sb.AppendLine("");
            sb.AppendLine(string.Format("        public async Task<FormatedResponse> CreateRange(List<{0}DTO> dtos, string sid)", PascalName));
            sb.AppendLine("        {");
            sb.AppendLine(string.Format("            var add = new List<{0}DTO>();", PascalName));
            sb.AppendLine("            add.AddRange(dtos);");
            sb.AppendLine("            var response = await _genericRepository.CreateRange(add, \"root\");");
            sb.AppendLine("            return response;");
            sb.AppendLine("        }");
            sb.AppendLine("");
            sb.AppendLine(string.Format("        public async Task<FormatedResponse> Update({0}DTO dto, string sid, bool patchMode = true)", PascalName));
            sb.AppendLine("        {");
            sb.AppendLine("            var response = await _genericRepository.Update(dto, \"root\", patchMode);");
            sb.AppendLine("            return response;");
            sb.AppendLine("        }");
            sb.AppendLine("");
            sb.AppendLine(string.Format("        public async Task<FormatedResponse> UpdateRange(List<{0}DTO> dtos, string sid, bool patchMode = true)", PascalName));
            sb.AppendLine("        {");
            sb.AppendLine("            var response = await _genericRepository.UpdateRange(dtos, \"root\", patchMode);");
            sb.AppendLine("            return response;");
            sb.AppendLine("        }");
            sb.AppendLine("");
            sb.AppendLine("        public async Task<FormatedResponse> Delete(long id)");
            sb.AppendLine("        {");
            sb.AppendLine("            var response = await _genericRepository.Delete(id);");
            sb.AppendLine("            return response;");
            sb.AppendLine("        }");
            sb.AppendLine("");
            sb.AppendLine("        public async Task<FormatedResponse> DeleteIds(List<long> ids)");
            sb.AppendLine("        {");
            sb.AppendLine("            var response = await _genericRepository.DeleteIds(ids);");
            sb.AppendLine("            return response;");
            sb.AppendLine("        }");
            sb.AppendLine("");            
            sb.AppendLine("        public async Task<FormatedResponse> ToggleActiveIds(List<long> ids, bool valueToBind, string sid)");
            sb.AppendLine("        {");
            sb.AppendLine("            throw new NotImplementedException();");
            sb.AppendLine("        }");
            sb.AppendLine("");
            sb.AppendLine("    }");
            sb.AppendLine("}");

            return sb.ToString();
        }

        public static string GenerateControllerClass(string SNAKE_NAME, string PascalName, string TwoDigitIndex, string ModuleCode)
        {
            StringBuilder sb = new();

            sb.AppendLine("using API;");
            sb.AppendLine("using GYM_BE.Core.Dto;");
            sb.AppendLine("using GYM_BE.DTO;");
            sb.AppendLine("using GYM_BE.Entities;");
            sb.AppendLine("using Microsoft.AspNetCore.Mvc;");
            sb.AppendLine("using Microsoft.EntityFrameworkCore;");
            sb.AppendLine("using Microsoft.Extensions.Options;");
            sb.AppendLine("");
            sb.AppendLine(string.Format("namespace GYM_BE.All.{0}", PascalName));
            sb.AppendLine("{");
            sb.AppendLine(string.Format("    [ApiExplorerSettings(GroupName = \"{0}-{1}-{2}\")]", TwoDigitIndex, ModuleCode, SNAKE_NAME));
            sb.AppendLine("    [ApiController]");
            sb.AppendLine("    [Route(\"api/[controller]/[action]\")]");
            sb.AppendLine(string.Format("    public class {0}Controller : Controller", PascalName));
            sb.AppendLine("    {");
            sb.AppendLine("        private readonly FullDbContext _dbContext;");
            sb.AppendLine(string.Format("        private readonly I{0}Repository _{0}Repository;", PascalName));
            sb.AppendLine("        private readonly AppSettings _appSettings;");
            sb.AppendLine("");
            sb.AppendLine(string.Format("        public {0}Controller(", PascalName));
            sb.AppendLine("            DbContextOptions<FullDbContext> dbOptions,");
            sb.AppendLine("            IOptions<AppSettings> options)");
            sb.AppendLine("        {");
            sb.AppendLine("            _dbContext = new FullDbContext(dbOptions, options);");
            sb.AppendLine(string.Format("            _{0}Repository = new {0}Repository(_dbContext);", PascalName));
            sb.AppendLine("            _appSettings = options.Value;");
            sb.AppendLine("        }");
            sb.AppendLine("");
            sb.AppendLine("        [HttpPost]");
            sb.AppendLine(string.Format("        public async Task<IActionResult> QueryList(PaginationDTO pagination)", PascalName));
            sb.AppendLine("        {");
            sb.AppendLine(string.Format("            var response = await _{0}Repository.QueryList(pagination);", PascalName));
            sb.AppendLine("            return Ok(response);");
            sb.AppendLine("        }");
            sb.AppendLine("");
            sb.AppendLine("        [HttpGet]");
            sb.AppendLine("        public async Task<IActionResult> GetById(long id)");
            sb.AppendLine("        {");
            sb.AppendLine(string.Format("            var response = await _{0}Repository.GetById(id);", PascalName));
            sb.AppendLine("            return Ok(response);");
            sb.AppendLine("        }");
            sb.AppendLine("");
            sb.AppendLine("        [HttpPost]");
            sb.AppendLine(string.Format("        public async Task<IActionResult> Create({0}DTO model)", PascalName));
            sb.AppendLine("        {");
            sb.AppendLine(string.Format("            var response = await _{0}Repository.Create(model, \"root\");", PascalName));
            sb.AppendLine("            return Ok(response);");
            sb.AppendLine("        }");
            sb.AppendLine("        [HttpPost]");
            sb.AppendLine(string.Format("        public async Task<IActionResult> CreateRange(List<{0}DTO> models)", PascalName));
            sb.AppendLine("        {");
            sb.AppendLine(string.Format("            var response = await _{0}Repository.CreateRange(models, \"root\");", PascalName));
            sb.AppendLine("            return Ok(response);");
            sb.AppendLine("        }");
            sb.AppendLine("");
            sb.AppendLine("        [HttpPost]");
            sb.AppendLine(string.Format("        public async Task<IActionResult> Update({0}DTO model)", PascalName));
            sb.AppendLine("        {");
            sb.AppendLine(string.Format("            var response = await _{0}Repository.Update(model, \"root\");", PascalName));
            sb.AppendLine("            return Ok(response);");
            sb.AppendLine("        }");
            sb.AppendLine("");
            sb.AppendLine("        [HttpPost]");
            sb.AppendLine(string.Format("        public async Task<IActionResult> UpdateRange(List<{0}DTO> models)", PascalName));
            sb.AppendLine("        {");
            sb.AppendLine(string.Format("            var response = await _{0}Repository.UpdateRange(models, \"root\");", PascalName));
            sb.AppendLine("            return Ok(response);");
            sb.AppendLine("        }");
            sb.AppendLine("");
            sb.AppendLine("        [HttpPost]");
            sb.AppendLine(string.Format("        public async Task<IActionResult> Delete({0}DTO model)", PascalName));
            sb.AppendLine("        {");
            sb.AppendLine("            if (model.Id != null)");
            sb.AppendLine("            {");
            sb.AppendLine(string.Format("                var response = await _{0}Repository.Delete((long)model.Id);", PascalName));
            sb.AppendLine("                return Ok(response);");
            sb.AppendLine("            }");
            sb.AppendLine("            else");
            sb.AppendLine("            {");
            sb.AppendLine("                return Ok(new FormatedResponse() { ErrorType = EnumErrorType.CATCHABLE, MessageCode = \"DELETE_REQUEST_NULL_ID\" });");
            sb.AppendLine("            }");
            sb.AppendLine("        }");
            sb.AppendLine("");
            sb.AppendLine("        [HttpPost]");
            sb.AppendLine("        public async Task<IActionResult> DeleteIds(IdsRequest model)");
            sb.AppendLine("        {");
            sb.AppendLine(string.Format("            var response = await _{0}Repository.DeleteIds(model.Ids);", PascalName));
            sb.AppendLine("            return Ok(response);");
            sb.AppendLine("        }");
            sb.AppendLine("");            
            sb.AppendLine("        [HttpPost]");
            sb.AppendLine("        public async Task<IActionResult> ToggleActiveIds(GenericToggleIsActiveDTO model)");
            sb.AppendLine("        {");
            sb.AppendLine(string.Format("            var response = await _{0}Repository.ToggleActiveIds(model.Ids, model.ValueToBind, \"root\");", PascalName));
            sb.AppendLine("            return Ok(response);");
            sb.AppendLine("        }");
            sb.AppendLine("");
            sb.AppendLine("    }");
            sb.AppendLine("}");

            return sb.ToString();
        }

        public static void GenerateFullDbContext(string Folder, List<MyTable> EntityList)
        {
            StringBuilder sb = new();

            sb.AppendLine("using Microsoft.Extensions.Options;");
            sb.AppendLine("");
            sb.AppendLine("namespace API.All.DbContexts");
            sb.AppendLine("{");
            sb.AppendLine("    public class FullDbContext : DbContextBase {");
            sb.AppendLine("");
            sb.AppendLine("        public FullDbContext(IConfiguration config, DbContextOptions<FullDbContext> options, IHttpContextAccessor accessor, IOptions<AppSettings> appSettings)");
            sb.AppendLine("            : base(config, options, accessor, appSettings) {}");
            sb.AppendLine("");

            EntityList.ForEach(table =>
            {
                string EntityName = table.TableName;
                sb.AppendLine(
                    string.Format(
                        "        public DbSet<{0}> {1}s {2} get; set; {3}", 
                        EntityName, 
                        EntityName.SnakeToCamelCase().CamelToPascalCase(),
                        "{", 
                        "}"));
                sb.AppendLine("");
            });

            sb.AppendLine("    }");
            sb.AppendLine("}");

            using (StreamWriter file = new(string.Format("{0}\\FullDbContext.cs", Folder)))
            {
                file.WriteLine(sb.ToString());
            }

        }
    }
}

