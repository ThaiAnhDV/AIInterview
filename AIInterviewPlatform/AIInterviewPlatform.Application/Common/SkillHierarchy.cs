namespace AIInterviewPlatform.Application.Common;

/// <summary>
/// Defines parent-child relationships for skills to prevent false positives.
/// Child skills satisfy parent skill requirements (e.g., SQL Server satisfies SQL),
/// but parent skills do NOT satisfy child skill requirements (e.g., SQL does NOT satisfy SQL Server).
/// </summary>
public static class SkillHierarchy
{
    private static readonly Dictionary<string, HashSet<string>> ParentToChildren = new(StringComparer.OrdinalIgnoreCase)
    {
        // Database hierarchy
        ["SQL"] = new(StringComparer.OrdinalIgnoreCase)
        {
            "SQL Server",
            "MySQL",
            "PostgreSQL",
            "Oracle Database",
            "SQLite",
            "MariaDB",
            "Amazon RDS",
            "Azure SQL",
            "Google Cloud SQL"
        },
        ["NoSQL"] = new(StringComparer.OrdinalIgnoreCase)
        {
            "MongoDB",
            "Cassandra",
            "DynamoDB",
            "Couchbase",
            "Redis",
            "Neo4j"
        },
        ["Document Database"] = new(StringComparer.OrdinalIgnoreCase)
        {
            "MongoDB",
            "Couchbase",
            "CouchDB",
            "Firestore"
        },
        ["Key-Value Store"] = new(StringComparer.OrdinalIgnoreCase)
        {
            "Redis",
            "Memcached",
            "DynamoDB"
        },

        // .NET ecosystem
        [".NET"] = new(StringComparer.OrdinalIgnoreCase)
        {
            "ASP.NET",
            "ASP.NET Core",
            "ASP.NET MVC",
            "WPF",
            "WinForms",
            ".NET MAUI",
            "Xamarin"
        },
        ["ASP.NET"] = new(StringComparer.OrdinalIgnoreCase)
        {
            "ASP.NET Core",
            "ASP.NET MVC",
            "ASP.NET Web Forms",
            "ASP.NET Web API"
        },
        [".NET Framework"] = new(StringComparer.OrdinalIgnoreCase)
        {
            ".NET Framework 4.8",
            ".NET Framework 4.7",
            ".NET Framework 4.6"
        },

        // ORM framework
        ["Entity Framework"] = new(StringComparer.OrdinalIgnoreCase)
        {
            "Entity Framework Core",
            "EF Core",
            "Entity Framework 6"
        },
        ["ORM"] = new(StringComparer.OrdinalIgnoreCase)
        {
            "Entity Framework",
            "Entity Framework Core",
            "Dapper",
            "NHibernate",
            "Hibernate",
            "Sequelize",
            "Prisma"
        },

        // JavaScript ecosystem
        ["JavaScript"] = new(StringComparer.OrdinalIgnoreCase)
        {
            "React",
            "Angular",
            "Vue.js",
            "Node.js",
            "Express",
            "Svelte",
            "Next.js",
            "Nuxt.js",
            "NestJS"
        },
        ["ECMAScript"] = new(StringComparer.OrdinalIgnoreCase)
        {
            "JavaScript",
            "TypeScript"
        },
        ["Frontend Framework"] = new(StringComparer.OrdinalIgnoreCase)
        {
            "React",
            "Angular",
            "Vue.js",
            "Svelte",
            "Next.js"
        },
        ["UI Framework"] = new(StringComparer.OrdinalIgnoreCase)
        {
            "React",
            "Angular",
            "Vue.js",
            "Svelte"
        },
        ["CSS Framework"] = new(StringComparer.OrdinalIgnoreCase)
        {
            "Bootstrap",
            "Tailwind CSS",
            "Material UI",
            "Ant Design",
            "Chakra UI"
        },

        // React ecosystem
        ["React"] = new(StringComparer.OrdinalIgnoreCase)
        {
            "React Native",
            "ReactJS",
            "Next.js"
        },

        // Mobile development
        ["Mobile Development"] = new(StringComparer.OrdinalIgnoreCase)
        {
            "React Native",
            "Flutter",
            "Swift",
            "Kotlin",
            "Xamarin",
            ".NET MAUI"
        },
        ["Cross-Platform Mobile"] = new(StringComparer.OrdinalIgnoreCase)
        {
            "React Native",
            "Flutter",
            "Xamarin",
            ".NET MAUI"
        },

        // Backend frameworks
        ["Backend Framework"] = new(StringComparer.OrdinalIgnoreCase)
        {
            "Express",
            "NestJS",
            "FastAPI",
            "Django",
            "Flask",
            "Spring Boot",
            "Rails",
            "Laravel"
        },
        ["Node.js Framework"] = new(StringComparer.OrdinalIgnoreCase)
        {
            "Express",
            "NestJS",
            "Fastify",
            "Koa"
        },

        // API technologies
        ["API"] = new(StringComparer.OrdinalIgnoreCase)
        {
            "REST API",
            "GraphQL",
            "gRPC",
            "Web API",
            "OData"
        },
        ["REST API"] = new(StringComparer.OrdinalIgnoreCase)
        {
            "RESTful API",
            "REST API Design"
        },

        // Cloud platforms
        ["Cloud"] = new(StringComparer.OrdinalIgnoreCase)
        {
            "AWS",
            "Azure",
            "GCP"
        },
        ["Cloud Platform"] = new(StringComparer.OrdinalIgnoreCase)
        {
            "AWS",
            "Azure",
            "GCP",
            "Heroku",
            "DigitalOcean"
        },
        ["AWS"] = new(StringComparer.OrdinalIgnoreCase)
        {
            "AWS Lambda",
            "Amazon EC2",
            "Amazon S3",
            "Amazon RDS",
            "Amazon DynamoDB",
            "Amazon SQS",
            "Amazon SNS",
            "AWS CloudFormation",
            "AWS CDK",
            "Amazon EKS",
            "Amazon ECS",
            "AWS Fargate",
            "AWS Step Functions",
            "Amazon API Gateway"
        },
        ["Azure"] = new(StringComparer.OrdinalIgnoreCase)
        {
            "Azure Functions",
            "Azure App Service",
            "Azure Cosmos DB",
            "Azure SQL",
            "Azure Storage",
            "Azure Service Bus",
            "Azure DevOps",
            "Azure Kubernetes Service",
            "Azure Logic Apps"
        },
        ["GCP"] = new(StringComparer.OrdinalIgnoreCase)
        {
            "Google Cloud Functions",
            "Google App Engine",
            "Google Cloud Run",
            "Google Cloud Storage",
            "Google Cloud SQL",
            "Firestore",
            "Google Cloud Pub/Sub",
            "Google Kubernetes Engine",
            "BigQuery"
        },

        // DevOps & Containers
        ["Container"] = new(StringComparer.OrdinalIgnoreCase)
        {
            "Docker",
            "Podman"
        },
        ["Container Orchestration"] = new(StringComparer.OrdinalIgnoreCase)
        {
            "Kubernetes",
            "Docker Swarm",
            "Amazon EKS",
            "Azure Kubernetes Service",
            "Google Kubernetes Engine"
        },
        ["CI/CD"] = new(StringComparer.OrdinalIgnoreCase)
        {
            "Jenkins",
            "GitHub Actions",
            "GitLab CI",
            "Azure DevOps",
            "CircleCI",
            "Travis CI"
        },
        ["DevOps"] = new(StringComparer.OrdinalIgnoreCase)
        {
            "Docker",
            "Kubernetes",
            "Jenkins",
            "GitHub Actions",
            "GitLab CI",
            "Terraform",
            "Ansible",
            "Azure DevOps"
        },

        // Programming languages (IMPORTANT: prevent Java vs JavaScript)
        ["Language"] = new(StringComparer.OrdinalIgnoreCase)
        {
            "C#",
            "Java",
            "Python",
            "JavaScript",
            "TypeScript",
            "Go",
            "Rust",
            "Ruby",
            "PHP",
            "Swift",
            "Kotlin",
            "Scala"
        },

        // Testing
        ["Testing"] = new(StringComparer.OrdinalIgnoreCase)
        {
            "Unit Testing",
            "Integration Testing",
            "End-to-End Testing",
            "TDD",
            "BDD"
        },
        ["Unit Testing"] = new(StringComparer.OrdinalIgnoreCase)
        {
            "xUnit",
            "NUnit",
            "Jest",
            "Mocha",
            "JUnit",
            "PyTest"
        },
        ["E2E Testing"] = new(StringComparer.OrdinalIgnoreCase)
        {
            "Selenium",
            "Cypress",
            "Playwright",
            "TestCafe"
        },

        // Architecture patterns
        ["Architecture Pattern"] = new(StringComparer.OrdinalIgnoreCase)
        {
            "Microservices",
            "Monolithic Architecture",
            "Clean Architecture",
            "Event-Driven Architecture"
        },
        ["Microservices"] = new(StringComparer.OrdinalIgnoreCase)
        {
            "API Gateway",
            "Service Mesh",
            "CQRS",
            "Event Sourcing"
        },

        // Message queues
        ["Message Queue"] = new(StringComparer.OrdinalIgnoreCase)
        {
            "RabbitMQ",
            "Kafka",
            "Amazon SQS",
            "Azure Service Bus",
            "Google Cloud Pub/Sub"
        },

        // Authentication & Security
        ["Authentication"] = new(StringComparer.OrdinalIgnoreCase)
        {
            "OAuth 2.0",
            "JWT",
            "Identity",
            "SAML"
        },
        ["Security"] = new(StringComparer.OrdinalIgnoreCase)
        {
            "OAuth 2.0",
            "JWT",
            "SAML",
            "OWASP"
        }
    };

    private static readonly Dictionary<string, HashSet<string>> ChildToParents;

    static SkillHierarchy()
    {
        // Build reverse mapping (child -> parents)
        ChildToParents = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);

        foreach (var (parent, children) in ParentToChildren)
        {
            foreach (var child in children)
            {
                if (!ChildToParents.ContainsKey(child))
                {
                    ChildToParents[child] = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                }
                ChildToParents[child].Add(parent);

                // Also add transitive parents
                if (ParentToChildren.TryGetValue(child, out var grandchildren))
                {
                    foreach (var grandchild in grandchildren)
                    {
                        if (!ChildToParents.ContainsKey(grandchild))
                        {
                            ChildToParents[grandchild] = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                        }
                        ChildToParents[grandchild].Add(parent);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Gets all direct children of a parent skill.
    /// </summary>
    public static IReadOnlyCollection<string> GetChildren(string parent)
    {
        if (ParentToChildren.TryGetValue(parent, out var children))
        {
            return children;
        }
        return Array.Empty<string>();
    }

    /// <summary>
    /// Gets all direct and indirect parents of a child skill.
    /// </summary>
    public static IReadOnlyCollection<string> GetParents(string child)
    {
        if (ChildToParents.TryGetValue(child, out var parents))
        {
            return parents;
        }
        return Array.Empty<string>();
    }

    /// <summary>
    /// Checks if skillB is a child (descendant) of skillA.
    /// </summary>
    public static bool IsChildOf(string skillB, string skillA)
    {
        if (string.Equals(skillA, skillB, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        var childrenOfA = GetChildren(skillA);
        if (childrenOfA.Contains(skillB))
        {
            return true;
        }

        // Check transitive children
        var visited = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var queue = new Queue<string>(childrenOfA);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            if (visited.Contains(current))
            {
                continue;
            }
            visited.Add(current);

            if (string.Equals(current, skillB, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            foreach (var grandchild in GetChildren(current))
            {
                if (!visited.Contains(grandchild))
                {
                    queue.Enqueue(grandchild);
                }
            }
        }

        return false;
    }

    /// <summary>
    /// Checks if two skills are in the same hierarchy (siblings or ancestor-descendant).
    /// </summary>
    public static bool AreInSameHierarchy(string skillA, string skillB)
    {
        if (string.Equals(skillA, skillB, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        // Direct parent-child relationship
        if (IsChildOf(skillA, skillB) || IsChildOf(skillB, skillA))
        {
            return true;
        }

        // Check if they share a common parent
        var parentsOfA = GetParents(skillA);
        var parentsOfB = GetParents(skillB);

        foreach (var parentA in parentsOfA)
        {
            foreach (var parentB in parentsOfB)
            {
                if (string.Equals(parentA, parentB, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
        }

        return false;
    }

    /// <summary>
    /// Adds a parent-child relationship to the hierarchy.
    /// </summary>
    public static void AddRelationship(string parent, string child)
    {
        if (!ParentToChildren.ContainsKey(parent))
        {
            ParentToChildren[parent] = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        }
        ParentToChildren[parent].Add(child);

        if (!ChildToParents.ContainsKey(child))
        {
            ChildToParents[child] = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        }
        ChildToParents[child].Add(parent);
    }
}
