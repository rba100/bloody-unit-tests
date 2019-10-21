using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BloodyUnitTests.Util;
using static System.StringComparison;

namespace BloodyUnitTests.ContentCreators
{
    public class DomainObjectTestCreator : IContentCreator
    {
        private readonly CSharpService m_CSharpService = new CSharpService();

        public ClassContent Create(Type type)
        {
            var ctrs = type.GetConstructors();
            if (ctrs.Length != 1) return ClassContent.NoContent;
            var parameters = ctrs.Single().GetParameters();
            if (!parameters.Any()) return ClassContent.NoContent;

            var properties = type.GetProperties();

            var pairs = properties.Select(p => (property: p,
                                                parameter: parameters.FirstOrDefault(
                                                    para => para.Name.Equals(p.Name,
                                                                             InvariantCultureIgnoreCase))))
                                  .Where(pair => pair.parameter != null).ToArray();

            if (parameters.Length != pairs.Length) return ClassContent.NoContent;

            var lines = new List<string>();
            var verificationLines = new List<string>();
            lines.Add("[Test]");
            lines.Add("public void Parameter_round_trip_test()");
            lines.Add("{");
            var instanceName = m_CSharpService.GetIdentifier(type, VarScope.Local);
            foreach (var pair in pairs)
            {
                var varName = StringUtils.ToLowerInitial(pair.parameter.Name);
                var initial = pair.parameter.ParameterType == typeof(string)
                    ? $"\"{pair.parameter.Name}\""
                    : m_CSharpService.GetInstantiation(pair.parameter.ParameterType, true);

                lines.Add($"    var {varName} = {initial};");
                verificationLines.Add($"    Assert.AreEqual({varName}, {instanceName}.{pair.property.Name});");
            }
            lines.Add("");
            var instanceDeclr = $"    var {instanceName} = new {m_CSharpService.GetNameForCSharp(type)}(";
            var instanceDeclrOffset = new string(' ', instanceDeclr.Length);
            var args = parameters.Select(p => p.Name).Select(StringUtils.ToLowerInitial).ToArray();
            for (var i = 0; i < args.Length; i++)
            {
                var terminator = i == args.Length - 1 ? ");" : ",";
                lines.Add(i == 0
                              ? $"{instanceDeclr}{args[i]}{terminator}"
                              : $"{instanceDeclrOffset}{args[i]}{terminator}");
            }
            lines.Add("");
            lines.AddRange(verificationLines);
            lines.Add("}");
            return new ClassContent(lines.ToArray(), m_CSharpService.GetNameSpaces());
        }
    }
}
