using AIInterviewPlatform.Application.Interfaces.Prompts;
using AIInterviewPlatform.Domain.Common;

namespace AIInterviewPlatform.Infrastructure.AI.Prompts.Providers;

public sealed class EnglishPromptProvider : IPromptProvider
{
    public string LanguageCode => SupportedLanguageCodes.English;

    public string BuildResumeSkillExtractionPrompt(string resumeContent)
    {
        return $$"""
Extract ONLY explicit skills from the resume.

Return ONLY valid JSON:
{"skills":[]}

Include only:
- hard skills
- technical skills
- tools
- software
- frameworks
- methodologies
- accounting standards
- audit concepts

Exclude:
- job responsibilities
- tasks
- achievements
- project descriptions
- activities
- soft skills

Do not return action phrases or duties such as:
- auditing processes
- report compilation
- error detection
- documentation preparation

Prefer canonical skill names such as:
- Auditing
- Accounting
- Financial Reporting
- Financial Statement Analysis
- Microsoft Excel
- Microsoft Word
- Internal Control
- Risk Assessment

Resume:
{{resumeContent}}
""";
    }

    public string BuildJobDescriptionSkillExtractionPrompt(string jobDescriptionContent)
    {
        return $$"""
Extract ONLY required skills from the job description.

Return ONLY valid JSON:
{"requiredSkills":[]}

Include only:
- hard skills
- technical skills
- tools
- software
- frameworks
- methodologies
- accounting standards
- audit concepts

Exclude:
- job responsibilities
- tasks
- achievements
- project descriptions
- activities
- soft skills

Do not return action phrases or duties.
Prefer canonical skill names.

Job Description:
{{jobDescriptionContent}}
""";
    }

    public string BuildInterviewQuestionPrompt(
        string targetJobTitle,
        string targetJobDescription,
        IReadOnlyCollection<string> requiredSkills,
        IReadOnlyCollection<string> missingSkills)
    {
        var focusAreas = missingSkills.Count > 0
            ? string.Join(", ", missingSkills)
            : string.Join(", ", requiredSkills);

        return $$"""
Generate 10 interview questions.

Position: {{targetJobTitle}}

Description:
{{targetJobDescription}}

Focus Areas:
{{focusAreas}}

Return JSON only:

{
  "questions": [
    {
      "category": "Technical",
      "skillFocus": "C#",
      "question": "..."
    }
  ]
}

Rules:
- Prioritize missing skills when available.
- Align questions to the target job description.
- Use categories such as Technical, Behavioral, and Communication when appropriate.
- Return valid JSON only.
- Do not include markdown fences or explanatory text.
""";
    }

    public string BuildInterviewEvaluationPrompt(string question, string answer, string? category, string? skillFocus)
    {
        return $$"""
You are a strict technical interviewer.

Evaluate the answer using the FULL range 0-100.

Question: {{question}}
Category: {{category ?? "General"}}
Skill: {{skillFocus ?? "Not specified"}}
Answer: {{answer}}

Scoring rubric:

0-20
Completely incorrect, irrelevant, or "I don't know"

21-40
Very weak answer with major knowledge gaps

41-60
Partial understanding but missing key concepts

61-75
Acceptable answer with noticeable weaknesses

76-89
Strong answer with good understanding

90-100
Excellent answer demonstrating deep knowledge and clear communication

Return JSON only:

{
  "clarityScore": 0,
  "technicalAccuracyScore": 0,
  "completenessScore": 0,
  "overallScore": 0,
  "strengths": [],
  "weaknesses": [],
  "feedback": ""
}

Rules:
- Use the entire score range.
- Do not avoid low scores.
- "I don't know" must score below 20.
- One-sentence answers should rarely exceed 40.
- Excellent answers may score 95-100.
- Return only valid JSON and no markdown fences.
""";
    }

    public string BuildActivityDescriptionPrompt(string skillName, string difficultyLevel)
    {
        return $$"""
Generate ONE practical learning activity for a software engineer who wants to learn {{skillName}}.

Requirements:
- Beginner friendly (suitable for someone with basic programming knowledge)
- Maximum 50 words for the description
- Software engineering focus
- Practical and hands-on
- activityType must be one of: READING, PRACTICE, MOCK_INTERVIEW, QUIZ, OTHER

Return ONLY a valid JSON object with this exact format (no markdown, no explanation, no prose before or after):
{"activityTitle": "short action-oriented title", "activityDescription": "concise description under 50 words", "activityType": "READING"}

Example activityTitle: "Build a REST API Calculator" or "Create a Docker container"
Example activityDescription: "Create a simple calculator REST API using Express.js. Implement basic operations: add, subtract, multiply, divide. Test endpoints with Postman."
Example activityType: "PRACTICE"

Skill: {{skillName}}
Difficulty: {{difficultyLevel}}

IMPORTANT:
- Return ONLY the JSON object.
- Do not wrap in markdown.
- Do not prefix with 'Here is the activity'.
- Do not include any text outside the JSON object.
""";
    }

    public string BuildAssistantChatPrompt(string page, string message)
    {
        return $$"""
You are a friendly AI assistant in an AI interview preparation platform.
Mission: explain clearly and briefly in English, helping the user understand what to do next in the product.
Do not ask for sensitive information, do not invent personal data, and do not answer as if you are conducting the interview itself.
If the question is unrelated to the platform, still help briefly, but steer the user back toward interview preparation.

Current page: {{page}}
User question: {{message}}

Reply in 2-5 sentences, and include 1-3 practical next steps if useful.
""";
    }

    public string GetAssistantFallbackMessage(string page)
    {
        page = (page ?? string.Empty).ToLowerInvariant();
        var reply = "The AI service is currently unavailable, but you can continue with the main flow: upload your resume, create a target job and job description, analyze skill gaps, then practice interviews.";

        if (page.Contains("resume"))
        {
            reply = "You are on the resume section. Upload your CV first, then mark the main resume so the system has data for skill analysis.";
        }
        else if (page.Contains("target"))
        {
            reply = "You are on the target job section. Create the role you want to apply for, then add the job description so AI can compare skill requirements.";
        }
        else if (page.Contains("skill"))
        {
            reply = "You are on the skill analysis section. Choose a resume and a job with a job description to see matched skills, missing skills, and learning priorities.";
        }
        else if (page.Contains("roadmap"))
        {
            reply = "You are on the roadmap section. Choose a skill gap analysis result to generate a personalized learning plan and track progress across activities.";
        }

        return reply;
    }

    public string GetAssistantEmptyMessage() => "Please type a question so the assistant can help you.";

    public string GetAssistantMissingApiKeyMessage() =>
        "The AI assistant is unavailable because Gemini API key is not configured. Add a real API key to GeminiSettings:ApiKey and restart the API.";
}
