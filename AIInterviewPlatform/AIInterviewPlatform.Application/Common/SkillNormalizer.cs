using System.Text.RegularExpressions;

namespace AIInterviewPlatform.Application.Common;

public static class SkillNormalizer
{
    private static readonly Dictionary<string, string> SkillAliases = new(StringComparer.OrdinalIgnoreCase)
    {
        // C# and .NET variants
        { "c#", "C#" },
        { "csharp", "C#" },
        { "c sharp", "C#" },
        { ".net", ".NET" },
        { "dot net", ".NET" },
        { "dotnet", ".NET" },
        { "asp.net", "ASP.NET Core" },
        { "asp.net core", "ASP.NET Core" },
        { "aspnetcore", "ASP.NET Core" },
        { "asp .net", "ASP.NET Core" },

        // C++ variants
        { "c++", "C++" },
        { "cpp", "C++" },
        { "c plus plus", "C++" },

        // JavaScript variants
        { "javascript", "JavaScript" },
        { "js", "JavaScript" },
        { "ecmascript", "JavaScript" },

        // TypeScript
        { "typescript", "TypeScript" },
        { "ts", "TypeScript" },

        // Python
        { "python", "Python" },
        { "py", "Python" },

        // Java
        { "java", "Java" },

        // Node.js
        { "nodejs", "Node.js" },
        { "node.js", "Node.js" },
        { "node js", "Node.js" },
        { "node", "Node.js" },

        // Database
        { "sql server", "SQL Server" },
        { "mssql", "SQL Server" },
        { "ms sql", "SQL Server" },
        { "sqlserver", "SQL Server" },
        { "postgresql", "PostgreSQL" },
        { "postgres", "PostgreSQL" },
        { "postgres sql", "PostgreSQL" },
        { "mysql", "MySQL" },
        { "mongodb", "MongoDB" },
        { "mongo db", "MongoDB" },
        { "mongo", "MongoDB" },
        { "redis", "Redis" },

        // Cloud platforms
        { "aws", "AWS" },
        { "amazon web services", "AWS" },
        { "amazon webservices", "AWS" },
        { "azure", "Azure" },
        { "microsoft azure", "Azure" },
        { "gcp", "GCP" },
        { "google cloud", "GCP" },
        { "google cloud platform", "GCP" },

        // Containers & DevOps
        { "docker", "Docker" },
        { "kubernetes", "Kubernetes" },
        { "k8s", "Kubernetes" },
        { "k8", "Kubernetes" },
        { "jenkins", "Jenkins" },
        { "ci/cd", "CI/CD" },
        { "cicd", "CI/CD" },
        { "devops", "DevOps" },

        // Frameworks
        { "react.js", "React" },
        { "reactjs", "React" },
        { "react js", "React" },
        { "vue.js", "Vue.js" },
        { "vuejs", "Vue.js" },
        { "vue js", "Vue.js" },
        { "vue", "Vue.js" },
        { "angular.js", "Angular" },
        { "angularjs", "Angular" },
        { "angular js", "Angular" },
        { "angular", "Angular" },
        { "next.js", "Next.js" },
        { "nextjs", "Next.js" },

        // ORM
        { "entity framework", "Entity Framework" },
        { "ef core", "Entity Framework" },
        { "ef", "Entity Framework" },
        { "dapper", "Dapper" },
        { "hibernate", "Hibernate" },
        { "sequelize", "Sequelize" },

        // API
        { "rest api", "REST API" },
        { "restapi", "REST API" },
        { "rest", "REST API" },
        { "restful", "REST API" },
        { "graphql", "GraphQL" },
        { "grpc", "gRPC" },
        { "web api", "Web API" },
        { "webapi", "Web API" },

        // Testing
        { "unit testing", "Unit Testing" },
        { "unit tests", "Unit Testing" },
        { "xunit", "xUnit" },
        { "nunit", "NUnit" },
        { "jest", "Jest" },
        { "selenium", "Selenium" },
        { "cypress", "Cypress" },
        { "playwright", "Playwright" },

        // Other tools
        { "git", "Git" },
        { "github", "GitHub" },
        { "gitlab", "GitLab" },
        { "bitbucket", "Bitbucket" },
        { "jira", "Jira" },
        { "confluence", "Confluence" },
        { "rabbitmq", "RabbitMQ" },
        { "kafka", "Kafka" },
        { "apache kafka", "Kafka" },
        { "microservices", "Microservices" },
        { "microservice", "Microservices" },
        { "microservice architecture", "Microservices" },

        // Web
        { "html", "HTML" },
        { "html5", "HTML" },
        { "css", "CSS" },
        { "css3", "CSS" },
        { "sass", "Sass" },
        { "scss", "SCSS" },
        { "less", "Less" },
        { "bootstrap", "Bootstrap" },
        { "tailwind", "Tailwind CSS" },
        { "tailwindcss", "Tailwind CSS" },

        // Methodologies
        { "agile", "Agile" },
        { "scrum", "Scrum" },
        { "kanban", "Kanban" },
        { "tdd", "TDD" },
        { "test driven development", "TDD" },
        { "bdd", "BDD" },
        { "behavior driven development", "BDD" },

        // Architecture
        { "clean architecture", "Clean Architecture" },
        { "domain driven design", "Domain-Driven Design" },
        { "ddd", "Domain-Driven Design" },
        { "cqrs", "CQRS" },
        { "event sourcing", "Event Sourcing" },
        { "solid principles", "SOLID" },
        { "solid", "SOLID" },

        // Data
        { "linq", "LINQ" },
        { "entity framework core", "Entity Framework" },
        { "sql", "SQL" },
        { "nosql", "NoSQL" },
        { "odata", "OData" },
        { "elasticsearch", "Elasticsearch" },

        // Mobile
        { "react native", "React Native" },
        { "flutter", "Flutter" },
        { "swift", "Swift" },
        { "kotlin", "Kotlin" },
        { "xamarin", "Xamarin" },

        // Security
        { "oauth", "OAuth" },
        { "oauth2", "OAuth 2.0" },
        { "oauth 2", "OAuth 2.0" },
        { "jwt", "JWT" },
        { "json web token", "JWT" },
        { "identityserver", "IdentityServer" },
        { "identity", "Identity" },

        // Message queues
        { "message queue", "Message Queue" },
        { "azure service bus", "Azure Service Bus" },
        { "service bus", "Azure Service Bus" },

        // Infrastructure
        { "linux", "Linux" },
        { "windows server", "Windows Server" },
        { "nginx", "Nginx" },
        { "apache", "Apache" },
        { "iis", "IIS" },
        { "terraform", "Terraform" },
        { "ansible", "Ansible" },
        { "chef", "Chef" },
        { "puppet", "Puppet" }
    };

    private static readonly HashSet<string> KnownSkills = new(StringComparer.OrdinalIgnoreCase)
    {
        "C#", ".NET", "ASP.NET Core", "C++", "Java", "Python", "JavaScript", "TypeScript",
        "Node.js", "Go", "Rust", "Ruby", "PHP", "Swift", "Kotlin", "Scala",
        "SQL Server", "PostgreSQL", "MySQL", "MongoDB", "Redis", "Elasticsearch",
        "AWS", "Azure", "GCP", "Docker", "Kubernetes", "Jenkins", "CI/CD", "DevOps",
        "React", "Angular", "Vue.js", "Next.js", "HTML", "CSS", "Tailwind CSS",
        "Entity Framework", "Dapper", "REST API", "GraphQL", "gRPC", "Web API",
        "Unit Testing", "xUnit", "NUnit", "Jest", "Selenium", "Cypress",
        "Git", "GitHub", "GitLab", "Jira", "RabbitMQ", "Kafka",
        "Microservices", "Clean Architecture", "Domain-Driven Design", "CQRS",
        "TDD", "BDD", "Agile", "Scrum", "SOLID", "LINQ",
        "OAuth 2.0", "JWT", "Identity", "IdentityServer",
        "Azure Service Bus", "Terraform", "Ansible", "Linux", "Nginx",
        "React Native", "Flutter", "NoSQL", "OData", "Event Sourcing"
    };

    public static string Normalize(string skill)
    {
        if (string.IsNullOrWhiteSpace(skill))
        {
            return string.Empty;
        }

        var trimmed = skill.Trim();
        
        var lowerInvariant = trimmed.ToUpperInvariant();
        
        if (SkillAliases.TryGetValue(lowerInvariant, out var normalized))
        {
            return normalized;
        }

        if (IsKnownSkill(trimmed))
        {
            return GetCanonicalForm(trimmed);
        }

        return TitleCase(trimmed);
    }

    public static List<string> NormalizeCollection(IEnumerable<string> skills)
    {
        return skills
            .Select(Normalize)
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(s => s)
            .ToList();
    }

    public static bool AreEquivalent(string skill1, string skill2)
    {
        if (string.IsNullOrWhiteSpace(skill1) || string.IsNullOrWhiteSpace(skill2))
        {
            return false;
        }

        var normalized1 = Normalize(skill1);
        var normalized2 = Normalize(skill2);

        return string.Equals(normalized1, normalized2, StringComparison.OrdinalIgnoreCase);
    }

    public static bool IsKnownSkill(string skill)
    {
        if (string.IsNullOrWhiteSpace(skill))
        {
            return false;
        }

        return KnownSkills.Contains(skill.Trim());
    }

    private static string GetCanonicalForm(string skill)
    {
        var upperInvariant = skill.Trim().ToUpperInvariant();
        
        foreach (var known in KnownSkills)
        {
            if (string.Equals(known.ToUpperInvariant(), upperInvariant, StringComparison.OrdinalIgnoreCase))
            {
                return known;
            }
        }

        return TitleCase(skill);
    }

    private static string TitleCase(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return text;
        }

        var words = text.Split([' ', '-', '_', '.'], StringSplitOptions.RemoveEmptyEntries);
        
        return string.Join(" ", words.Select(word =>
        {
            if (word.Length <= 2)
            {
                return word.ToUpperInvariant();
            }
            
            return char.ToUpperInvariant(word[0]) + word[1..].ToLowerInvariant();
        }));
    }

    public static double CalculateSimilarity(string skill1, string skill2)
    {
        if (string.IsNullOrWhiteSpace(skill1) || string.IsNullOrWhiteSpace(skill2))
        {
            return 0;
        }

        var s1 = skill1.Trim().ToLowerInvariant();
        var s2 = skill2.Trim().ToLowerInvariant();

        if (s1 == s2)
        {
            return 1.0;
        }

        if (AreEquivalent(skill1, skill2))
        {
            return 1.0;
        }

        var distance = LevenshteinDistance(s1, s2);
        var maxLength = Math.Max(s1.Length, s2.Length);
        
        if (maxLength == 0)
        {
            return 1.0;
        }

        return 1.0 - (double)distance / maxLength;
    }

    private static int LevenshteinDistance(string s1, string s2)
    {
        var m = s1.Length;
        var n = s2.Length;
        
        var d = new int[m + 1, n + 1];

        for (var i = 0; i <= m; i++)
        {
            d[i, 0] = i;
        }

        for (var j = 0; j <= n; j++)
        {
            d[0, j] = j;
        }

        for (var j = 1; j <= n; j++)
        {
            for (var i = 1; i <= m; i++)
            {
                var cost = s1[i - 1] == s2[j - 1] ? 0 : 1;

                d[i, j] = Math.Min(
                    Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                    d[i - 1, j - 1] + cost);
            }
        }

        return d[m, n];
    }
}
