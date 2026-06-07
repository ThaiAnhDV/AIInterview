using AIInterviewPlatform.Application.Interfaces.Prompts;
using AIInterviewPlatform.Domain.Common;

namespace AIInterviewPlatform.Infrastructure.AI.Prompts.Providers;

public sealed class VietnamesePromptProvider : IPromptProvider
{
    public string LanguageCode => SupportedLanguageCodes.Vietnamese;

    public string BuildResumeSkillExtractionPrompt(string resumeContent)
    {
        return $$"""
Chỉ trích xuất các kỹ năng được nêu rõ trong CV.

Chỉ trả về JSON hợp lệ:
{"skills":[]}

Chỉ bao gồm:
- kỹ năng cứng
- kỹ năng kỹ thuật
- công cụ
- phần mềm
- framework
- phương pháp
- chuẩn mực kế toán
- khái niệm kiểm toán

Loại trừ:
- trách nhiệm công việc
- nhiệm vụ
- thành tích
- mô tả dự án
- hoạt động
- kỹ năng mềm

Không trả về các cụm hành động hoặc nhiệm vụ như:
- quy trình kiểm toán
- tổng hợp báo cáo
- phát hiện lỗi
- chuẩn bị tài liệu

Ưu tiên tên kỹ năng chuẩn hóa như:
- Auditing
- Accounting
- Financial Reporting
- Financial Statement Analysis
- Microsoft Excel
- Microsoft Word
- Internal Control
- Risk Assessment

CV:
{{resumeContent}}
""";
    }

    public string BuildJobDescriptionSkillExtractionPrompt(string jobDescriptionContent)
    {
        return $$"""
Chỉ trích xuất các kỹ năng bắt buộc từ mô tả công việc.

Chỉ trả về JSON hợp lệ:
{"requiredSkills":[]}

Chỉ bao gồm:
- kỹ năng cứng
- kỹ năng kỹ thuật
- công cụ
- phần mềm
- framework
- phương pháp
- chuẩn mực kế toán
- khái niệm kiểm toán

Loại trừ:
- trách nhiệm công việc
- nhiệm vụ
- thành tích
- mô tả dự án
- hoạt động
- kỹ năng mềm

Không trả về các cụm hành động hoặc nhiệm vụ.
Ưu tiên tên kỹ năng chuẩn hóa.

Mô tả công việc:
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
Hãy tạo 10 câu hỏi phỏng vấn.

Vị trí: {{targetJobTitle}}

Mô tả:
{{targetJobDescription}}

Các trọng tâm ưu tiên:
{{focusAreas}}

Chỉ trả về JSON:

{
  "questions": [
    {
      "category": "Technical",
      "skillFocus": "C#",
      "question": "..."
    }
  ]
}

Quy tắc:
- Ưu tiên kỹ năng còn thiếu khi có.
- Bám sát yêu cầu công việc.
- Có thể dùng các nhóm như Technical, Behavioral, Communication khi phù hợp.
- Chỉ trả về JSON hợp lệ.
- Không dùng markdown hay thêm giải thích.
- Nội dung câu hỏi phải bằng tiếng Việt.
""";
    }

    public string BuildInterviewEvaluationPrompt(string question, string answer, string? category, string? skillFocus)
    {
        return $$"""
Bạn là một người phỏng vấn kỹ thuật nghiêm khắc.

Hãy đánh giá câu trả lời trên toàn bộ thang điểm 0-100.

Câu hỏi: {{question}}
Danh mục: {{category ?? "General"}}
Kỹ năng trọng tâm: {{skillFocus ?? "Not specified"}}
Câu trả lời: {{answer}}

Thang điểm:

0-20
Hoàn toàn sai, không liên quan, hoặc trả lời kiểu "Tôi không biết"

21-40
Câu trả lời rất yếu với nhiều lỗ hổng kiến thức lớn

41-60
Có hiểu một phần nhưng thiếu các khái niệm quan trọng

61-75
Câu trả lời chấp nhận được nhưng còn các điểm yếu rõ rệt

76-89
Câu trả lời tốt, thể hiện hiểu biết khá vững

90-100
Câu trả lời xuất sắc, thể hiện kiến thức sâu và giao tiếp rõ ràng

Chỉ trả về JSON:

{
  "clarityScore": 0,
  "technicalAccuracyScore": 0,
  "completenessScore": 0,
  "overallScore": 0,
  "strengths": [],
  "weaknesses": [],
  "feedback": ""
}

Quy tắc:
- Dùng toàn bộ thang điểm.
- Không né điểm thấp.
- "Tôi không biết" phải dưới 20 điểm.
- Câu trả lời một câu hiếm khi vượt quá 40 điểm.
- Câu trả lời xuất sắc có thể đạt 95-100.
- Chỉ trả về JSON hợp lệ, không dùng markdown.
- Nội dung strengths, weaknesses, feedback phải bằng tiếng Việt.
""";
    }

    public string BuildActivityDescriptionPrompt(string skillName, string difficultyLevel)
    {
        return $$"""
Hãy tạo MỘT hoạt động học tập thực tế cho một kỹ sư phần mềm muốn học {{skillName}}.

Yêu cầu:
- Phù hợp cho người mới bắt đầu (đã có kiến thức lập trình cơ bản)
- Mô tả tối đa 50 từ
- Tập trung vào kỹ thuật phần mềm
- Mang tính thực hành
- activityType phải là một trong các giá trị: READING, PRACTICE, MOCK_INTERVIEW, QUIZ, OTHER

Chỉ trả về một đối tượng JSON hợp lệ với đúng định dạng này (không markdown, không giải thích, không có nội dung ngoài JSON):
{"activityTitle": "tiêu đề ngắn mang tính hành động", "activityDescription": "mô tả ngắn gọn dưới 50 từ", "activityType": "READING"}

Ví dụ activityTitle: "Xây dựng REST API Calculator" hoặc "Tạo Docker container đầu tiên"
Ví dụ activityDescription: "Tạo một REST API máy tính đơn giản. Cài đặt phép cộng, trừ, nhân, chia và kiểm thử endpoint bằng Postman."
Ví dụ activityType: "PRACTICE"

Kỹ năng: {{skillName}}
Độ khó: {{difficultyLevel}}

QUAN TRỌNG:
- Chỉ trả về JSON.
- Không bọc trong markdown.
- Không thêm lời dẫn như 'Đây là hoạt động'.
- Nội dung activityTitle và activityDescription phải bằng tiếng Việt.
""";
    }

    public string BuildAssistantChatPrompt(string page, string message)
    {
        return $$"""
Bạn là trợ lý AI thân thiện trong nền tảng luyện phỏng vấn AI.
Nhiệm vụ: giải thích ngắn gọn, dễ hiểu, bằng tiếng Việt, giúp người dùng biết nên làm gì tiếp theo trong sản phẩm.
Không hỏi thông tin nhạy cảm, không bịa dữ liệu cá nhân, không trả lời như đang phỏng vấn trực tiếp.
Nếu câu hỏi không liên quan nền tảng, vẫn hỗ trợ ngắn gọn nhưng ưu tiên hướng người dùng quay lại mục tiêu luyện phỏng vấn.

Trang hiện tại: {{page}}
Câu hỏi người dùng: {{message}}

Trả lời trong 2-5 câu, có thể đưa 1-3 bước cụ thể nếu phù hợp.
""";
    }

    public string GetAssistantFallbackMessage(string page)
    {
        page = (page ?? string.Empty).ToLowerInvariant();
        var reply = "Hiện tại AI chưa kết nối được, nhưng bạn có thể tiếp tục theo luồng chính: tải hồ sơ, tạo công việc mục tiêu và JD, phân tích kỹ năng, rồi luyện phỏng vấn.";

        if (page.Contains("resume"))
        {
            reply = "Bạn đang ở phần hồ sơ. Hãy tải CV lên trước, sau đó chọn hồ sơ chính để hệ thống có dữ liệu phân tích kỹ năng.";
        }
        else if (page.Contains("target"))
        {
            reply = "Bạn đang ở phần công việc mục tiêu. Hãy tạo vị trí muốn ứng tuyển, rồi thêm JD để AI biết yêu cầu kỹ năng cần so sánh.";
        }
        else if (page.Contains("skill"))
        {
            reply = "Bạn đang ở phần phân tích kỹ năng. Hãy chọn hồ sơ và công việc đã có JD để xem kỹ năng phù hợp, kỹ năng còn thiếu và hướng ưu tiên học tập.";
        }
        else if (page.Contains("roadmap"))
        {
            reply = "Bạn đang ở phần lộ trình. Hãy chọn kết quả phân tích kỹ năng để tạo kế hoạch học tập cá nhân hóa và theo dõi tiến độ từng hoạt động.";
        }

        return reply;
    }

    public string GetAssistantEmptyMessage() => "Bạn hãy nhập câu hỏi để trợ lý có thể hỗ trợ nhé.";

    public string GetAssistantMissingApiKeyMessage() =>
        "Trợ lý AI chưa kết nối được vì Gemini API key chưa được cấu hình. Hãy thêm API key thật vào GeminiSettings:ApiKey rồi khởi động lại API.";
}
