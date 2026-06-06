(function () {
    "use strict";

    const STORAGE_KEY = "appLanguage";
    const DEFAULT_LANGUAGE = "vi";

    const vi = {
        "AI Interview Platform": "Nền tảng phỏng vấn AI",
        "Dashboard": "Bảng điều khiển",
        "Resume": "Hồ sơ",
        "My Resume": "Hồ sơ của tôi",
        "Target Jobs": "Công việc mục tiêu",
        "Target Job": "Công việc mục tiêu",
        "Target Job Description": "Mô tả công việc mục tiêu",
        "Job Description": "Mô tả công việc",
        "Interview": "Phỏng vấn",
        "Skill Analysis": "Phân tích kỹ năng",
        "Skill Gap Analysis": "Phân tích thiếu hụt kỹ năng",
        "Roadmap": "Lộ trình",
        "Learning Roadmap": "Lộ trình học tập",
        "Pricing": "Bảng giá",
        "Profile": "Hồ sơ cá nhân",
        "Language": "Ngôn ngữ",
        "Login": "Đăng nhập",
        "Logout": "Đăng xuất",
        "Register": "Đăng ký",
        "Password": "Mật khẩu",
        "E-mail": "Email",
        "Email": "Email",
        "Forgot Password?": "Quên mật khẩu?",
        "Sign in with Google": "Đăng nhập với Google",
        "Sign in with Apple": "Đăng nhập với Apple",
        "Already have an account?": "Đã có tài khoản?",
        "Get Started": "Bắt đầu",
        "Start Your Journey": "Bắt đầu hành trình",
        "AI-Powered Interview Preparation": "Luyện phỏng vấn cùng AI",
        "AI-Powered Interview Preparation Platform": "Nền tảng luyện phỏng vấn cùng AI",
        "Upload Resume": "Tải hồ sơ lên",
        "Choose Target Job": "Chọn công việc mục tiêu",
        "Mock Interview": "Phỏng vấn thử",
        "AI Analysis": "Phân tích AI",
        "Improve & Get Hired": "Cải thiện và ứng tuyển thành công",
        "Simple Process": "Quy trình đơn giản",
        "How It Works": "Cách hoạt động",
        "Powerful Features": "Tính năng nổi bật",
        "Everything You Need to Succeed": "Mọi thứ bạn cần để thành công",
        "Resume Analysis": "Phân tích hồ sơ",
        "Skill extraction": "Trích xuất kỹ năng",
        "ATS optimization tips": "Gợi ý tối ưu ATS",
        "Keyword analysis": "Phân tích từ khóa",
        "Matched skills highlighting": "Làm nổi bật kỹ năng phù hợp",
        "Missing skills detection": "Phát hiện kỹ năng còn thiếu",
        "Gap prioritization": "Ưu tiên khoảng cách kỹ năng",
        "AI Mock Interview": "Phỏng vấn thử với AI",
        "Real-time responses": "Phản hồi theo thời gian thực",
        "Detailed feedback": "Phản hồi chi tiết",
        "Multiple domains": "Nhiều lĩnh vực",
        "Personalized Roadmap": "Lộ trình cá nhân hóa",
        "Milestone tracking": "Theo dõi cột mốc",
        "AI-generated activities": "Hoạt động do AI tạo",
        "Progress analytics": "Phân tích tiến độ",
        "See It In Action": "Xem cách hoạt động",
        "Experience the Platform": "Trải nghiệm nền tảng",
        "Readiness Score": "Điểm sẵn sàng",
        "Ready": "Sẵn sàng",
        "Skills to Improve": "Kỹ năng cần cải thiện",
        "Completed": "Hoàn thành",
        "In Progress": "Đang thực hiện",
        "Pending": "Đang chờ",
        "AI Interviewer": "Người phỏng vấn AI",
        "Practice Makes Perfect": "Luyện tập để tiến bộ",
        "Online": "Đang trực tuyến",
        "Interview Feedback": "Nhận xét phỏng vấn",
        "Ready to Ace Your Next Interview?": "Sẵn sàng chinh phục buổi phỏng vấn tiếp theo?",
        "Readiness Score Card": "Thẻ điểm sẵn sàng",
        "No Dashboard Data Yet": "Chưa có dữ liệu bảng điều khiển",
        "Upload your resume and complete a Skill Gap Analysis to start tracking your progress.": "Tải hồ sơ lên và hoàn thành phân tích kỹ năng để bắt đầu theo dõi tiến độ.",
        "Analyze Skill Gap": "Phân tích kỹ năng còn thiếu",
        "Missing Skills": "Kỹ năng còn thiếu",
        "Total Interviews": "Tổng số phỏng vấn",
        "Avg Score": "Điểm trung bình",
        "Interview Stats": "Thống kê phỏng vấn",
        "Start Interview": "Bắt đầu phỏng vấn",
        "View Roadmap": "Xem lộ trình",
        "Recent Feedback": "Phản hồi gần đây",
        "Analyze Your Skills": "Phân tích kỹ năng của bạn",
        "Matched Skills": "Kỹ năng phù hợp",
        "Required Skills": "Kỹ năng yêu cầu",
        "Extracted from Job Description": "Trích xuất từ mô tả công việc",
        "Save Job Description": "Lưu mô tả công việc",
        "Extract Skills": "Trích xuất kỹ năng",
        "Question": "Câu hỏi",
        "Answer": "Câu trả lời",
        "Score": "Điểm",
        "Recommendation": "Đề xuất",
        "Recommendations": "Đề xuất",
        "Feedback": "Phản hồi",
        "Preview": "Xem trước",
        "Download": "Tải xuống",
        "Save": "Lưu",
        "Submit": "Gửi",
        "Start": "Bắt đầu",
        "Cancel": "Hủy",
        "Delete": "Xóa",
        "Edit": "Sửa",
        "Update": "Cập nhật",
        "Create": "Tạo",
        "Success": "Thành công",
        "Error": "Lỗi",
        "Loading": "Đang tải",
        "Privacy Policy": "Chính sách bảo mật",
        "Terms of Service": "Điều khoản dịch vụ",
        "Contact": "Liên hệ",
        "All rights reserved.": "Đã đăng ký bản quyền.",
        "Version": "Phiên bản",
        "Account created successfully!": "Tạo tài khoản thành công!",
        "Register failed!": "Đăng ký thất bại!",
        "Full name is required.": "Vui lòng nhập họ tên.",
        "Email is required.": "Vui lòng nhập email.",
        "Invalid email format.": "Email không đúng định dạng.",
        "Password is required.": "Vui lòng nhập mật khẩu.",
        "Password must be at least 6 characters.": "Mật khẩu phải có ít nhất 6 ký tự.",
        "Please accept Privacy Policy.": "Vui lòng chấp nhận Chính sách bảo mật.",
        "Cannot connect to server!": "Không thể kết nối tới máy chủ!",
        "Please enter email and password.": "Vui lòng nhập email và mật khẩu.",
        "Email format is invalid.": "Email không đúng định dạng.",
        "Login failed: invalid server response.": "Đăng nhập thất bại: phản hồi máy chủ không hợp lệ.",
        "Login successfully!": "Đăng nhập thành công!",
        "Login failed.": "Đăng nhập thất bại.",
        "Interview completed.": "Đã hoàn thành phỏng vấn.",
        "Answers submitted successfully.": "Gửi câu trả lời thành công.",
        "Skills extracted successfully!": "Trích xuất kỹ năng thành công!",
        "Profile updated successfully!": "Cập nhật hồ sơ thành công!",
        "Upload resume failed!": "Tải hồ sơ lên thất bại!",
        "Resume uploaded successfully!": "Tải hồ sơ lên thành công!",
        "Resume deleted successfully!": "Xóa hồ sơ thành công!",
        "Skill gap analysis completed!": "Phân tích kỹ năng hoàn tất!",
        "No resume skills found.": "Không tìm thấy kỹ năng trong hồ sơ.",
        "No required skills found.": "Không tìm thấy kỹ năng yêu cầu.",
        "No matched skills.": "Không có kỹ năng phù hợp.",
        "No missing skills. Great!": "Không thiếu kỹ năng nào. Rất tốt!",
        "Untitled Roadmap": "Lộ trình chưa đặt tên",
        "Not Started": "Chưa bắt đầu",
        "Please select a Skill Gap Analysis.": "Vui lòng chọn một phân tích kỹ năng.",
        "Roadmap generated successfully.": "Tạo lộ trình thành công."
    };

    const viExtra = {
        "Use this page to detail your site's privacy policy.": "Dùng trang này để mô tả chính sách bảo mật của website.",
        "An error occurred while processing your request.": "Đã xảy ra lỗi khi xử lý yêu cầu của bạn.",
        "Request ID:": "Mã yêu cầu:",
        "Development Mode": "Chế độ phát triển",
        "Pricing Plans": "Gói dịch vụ",
        "Choose Your Plan": "Chọn gói phù hợp",
        "Unlock powerful AI interview preparation features.": "Mở khóa các tính năng luyện phỏng vấn AI mạnh mẽ.",
        "Free": "Miễn phí",
        "Premium": "Cao cấp",
        "Most Popular": "Phổ biến nhất",
        "Current Plan": "Gói hiện tại",
        "Upgrade Now": "Nâng cấp ngay",
        "/ month": "/ tháng",
        "1 Resume Upload": "Tải 1 hồ sơ",
        "Basic Skill Analysis": "Phân tích kỹ năng cơ bản",
        "3 Mock Interviews / Month": "3 buổi phỏng vấn thử / tháng",
        "Basic Roadmap": "Lộ trình cơ bản",
        "Unlimited Resume Uploads": "Tải hồ sơ không giới hạn",
        "Advanced Skill Gap Analysis": "Phân tích thiếu hụt kỹ năng nâng cao",
        "Unlimited AI Mock Interviews": "Phỏng vấn thử với AI không giới hạn",
        "Detailed Feedback & Scores": "Phản hồi và điểm số chi tiết",
        "Priority Support": "Hỗ trợ ưu tiên",
        "Loading dashboard...": "Đang tải bảng điều khiển...",
        "Unable to load dashboard data": "Không thể tải dữ liệu bảng điều khiển",
        "Something went wrong while loading your dashboard.": "Đã xảy ra lỗi khi tải bảng điều khiển.",
        "Retry": "Thử lại",
        "Retrying...": "Đang thử lại...",
        "Based on your latest analysis": "Dựa trên phân tích mới nhất",
        "Current": "Hiện tại",
        "Previous": "Trước đó",
        "Critical": "Nghiêm trọng",
        "High": "Cao",
        "Medium": "Trung bình",
        "Highest": "Cao nhất",
        "Lowest": "Thấp nhất",
        "Roadmaps": "Lộ trình",
        "Progress": "Tiến độ",
        "Milestones": "Cột mốc",
        "Improving": "Đang cải thiện",
        "Declining": "Đang giảm",
        "Stable": "Ổn định",
        "First analysis": "Phân tích đầu tiên",
        "N/A": "Chưa có",
        "Upload and manage your resumes for interview analysis.": "Tải lên và quản lý hồ sơ để phân tích phỏng vấn.",
        "Upload New Resume": "Tải hồ sơ mới",
        "Your Resumes": "Hồ sơ của bạn",
        "File Name": "Tên tệp",
        "Status": "Trạng thái",
        "Uploaded At": "Ngày tải lên",
        "View": "Xem",
        "Actions": "Thao tác",
        "Active": "Đang dùng",
        "Set Active": "Chọn làm hồ sơ chính",
        "No resumes uploaded yet.": "Bạn chưa tải hồ sơ nào.",
        "Please select a resume file!": "Vui lòng chọn tệp hồ sơ!",
        "Cannot load resumes!": "Không thể tải danh sách hồ sơ!",
        "Cannot open resume!": "Không thể mở hồ sơ!",
        "Pop-up blocked. Please allow pop-ups to view the resume.": "Trình duyệt đang chặn cửa sổ bật lên. Vui lòng cho phép pop-up để xem hồ sơ.",
        "Cannot set active resume!": "Không thể chọn hồ sơ chính!",
        "Active resume updated!": "Đã cập nhật hồ sơ chính!",
        "Cannot delete resume!": "Không thể xóa hồ sơ!",
        "Create Target Job": "Tạo công việc mục tiêu",
        "My Target Jobs": "Công việc mục tiêu của tôi",
        "Total Jobs": "Tổng công việc",
        "With JD": "Đã có JD",
        "Pending JD": "Chưa có JD",
        "Enter the job position you're targeting": "Nhập vị trí công việc bạn đang hướng tới",
        "Optional industry specification": "Ngành nghề tùy chọn",
        "Years of experience required": "Số năm kinh nghiệm yêu cầu",
        "Select level...": "Chọn cấp bậc...",
        "Entry Level": "Mới vào nghề",
        "Junior": "Junior",
        "Mid-Level": "Trung cấp",
        "Senior": "Senior",
        "Lead": "Trưởng nhóm",
        "Manager": "Quản lý",
        "Loading target jobs...": "Đang tải công việc mục tiêu...",
        "No Target Jobs Yet": "Chưa có công việc mục tiêu",
        "Create your first target job above to start analyzing skills and preparing for interviews.": "Tạo công việc mục tiêu đầu tiên ở trên để bắt đầu phân tích kỹ năng và luyện phỏng vấn.",
        "Confirm Delete": "Xác nhận xóa",
        "Are you sure you want to delete": "Bạn có chắc muốn xóa",
        "this target job": "công việc mục tiêu này",
        "This action cannot be undone.": "Thao tác này không thể hoàn tác.",
        "Close": "Đóng",
        "Creating...": "Đang tạo...",
        "Deleting...": "Đang xóa...",
        "Cannot create target job!": "Không thể tạo công việc mục tiêu!",
        "Target job created successfully!": "Tạo công việc mục tiêu thành công!",
        "Cannot load target jobs!": "Không thể tải công việc mục tiêu!",
        "Cannot delete target job!": "Không thể xóa công việc mục tiêu!",
        "Target job deleted successfully!": "Xóa công việc mục tiêu thành công!",
        "Job description cannot be empty!": "Mô tả công việc không được để trống!",
        "Job description is required!": "Vui lòng nhập mô tả công việc!",
        "Cannot save job description!": "Không thể lưu mô tả công việc!",
        "Job description saved successfully!": "Lưu mô tả công việc thành công!",
        "Please save job description first!": "Vui lòng lưu mô tả công việc trước!",
        "Cannot extract skills!": "Không thể trích xuất kỹ năng!",
        "Target job id is missing!": "Thiếu mã công việc mục tiêu!",
        "Configure Interview": "Cấu hình buổi phỏng vấn",
        "-- Select Target Job --": "-- Chọn công việc mục tiêu --",
        "-- Select Analysis --": "-- Chọn phân tích --",
        "Go to Target Jobs →": "Đi tới Công việc mục tiêu →",
        "Go to Analysis →": "Đi tới Phân tích →",
        "Generating Interview Questions...": "Đang tạo câu hỏi phỏng vấn...",
        "Our AI is crafting personalized questions based on your profile.": "AI đang tạo câu hỏi cá nhân hóa dựa trên hồ sơ của bạn.",
        "Interview Summary": "Tóm tắt buổi phỏng vấn",
        "Job Position": "Vị trí công việc",
        "Session ID": "Mã phiên",
        "Total": "Tổng",
        "Technical": "Kỹ thuật",
        "Behavioral": "Hành vi",
        "Communication": "Giao tiếp",
        "Answer Questions": "Trả lời câu hỏi",
        "Answered": "Đã trả lời",
        "Something went wrong": "Đã xảy ra lỗi",
        "Answers Submitted Successfully!": "Gửi câu trả lời thành công!",
        "Your interview answers have been recorded. View feedback to improve.": "Câu trả lời của bạn đã được ghi nhận. Xem phản hồi để cải thiện.",
        "Type your answer here... (minimum 10 characters for best results)": "Nhập câu trả lời tại đây... (tối thiểu 10 ký tự để có kết quả tốt nhất)",
        "No target jobs found": "Không tìm thấy công việc mục tiêu",
        "No target jobs available": "Chưa có công việc mục tiêu",
        "Cannot load target jobs.": "Không thể tải công việc mục tiêu.",
        "Cannot connect to server.": "Không thể kết nối tới máy chủ.",
        "Please select a target job.": "Vui lòng chọn công việc mục tiêu.",
        "Cannot start interview.": "Không thể bắt đầu phỏng vấn.",
        "Cannot load interview session.": "Không thể tải phiên phỏng vấn.",
        "Cannot complete interview.": "Không thể hoàn thành phỏng vấn.",
        "Analysis Result": "Kết quả phân tích",
        "Loading your data...": "Đang tải dữ liệu của bạn...",
        "This target job has no job description!": "Công việc mục tiêu này chưa có mô tả công việc!",
        "Please select a resume!": "Vui lòng chọn hồ sơ!",
        "Cannot analyze skill gap!": "Không thể phân tích kỹ năng!",
        "Skill Gap Dashboard": "Bảng theo dõi kỹ năng",
        "Track your interview readiness and skill development": "Theo dõi mức sẵn sàng phỏng vấn và quá trình phát triển kỹ năng",
        "From latest analysis": "Từ phân tích mới nhất",
        "Skills to develop": "Kỹ năng cần phát triển",
        "Trend": "Xu hướng",
        "No Data Yet": "Chưa có dữ liệu",
        "Complete a skill gap analysis to see your history": "Hoàn thành phân tích kỹ năng để xem lịch sử",
        "No Matched Skills": "Chưa có kỹ năng phù hợp",
        "Complete an analysis to see your matched skills": "Hoàn thành phân tích để xem kỹ năng phù hợp",
        "No Missing Skills": "Không có kỹ năng còn thiếu",
        "Complete an analysis to see skill gaps": "Hoàn thành phân tích để xem khoảng cách kỹ năng",
        "Generate New Roadmap": "Tạo lộ trình mới",
        "Loading analyses...": "Đang tải phân tích...",
        "No Learning Roadmap Yet": "Chưa có lộ trình học tập",
        "Generate a Skill Gap Analysis first to receive a personalized roadmap.": "Hãy tạo phân tích kỹ năng trước để nhận lộ trình cá nhân hóa.",
        "Roadmap Title": "Tên lộ trình",
        "No analyses available": "Chưa có phân tích nào",
        "Select a skill gap analysis": "Chọn một phân tích kỹ năng",
        "Generating...": "Đang tạo...",
        "Cannot load roadmaps.": "Không thể tải lộ trình.",
        "Cannot load roadmap detail.": "Không thể tải chi tiết lộ trình.",
        "Cannot load skill gap analyses.": "Không thể tải các phân tích kỹ năng.",
        "Form element not found.": "Không tìm thấy form.",
        "Button element not found.": "Không tìm thấy nút.",
        "Cannot generate roadmap.": "Không thể tạo lộ trình.",
        "Cannot complete activity.": "Không thể hoàn thành hoạt động.",
        "Activity completed.": "Hoàn thành hoạt động.",
        "Preparing for better interviews": "Đang chuẩn bị cho những buổi phỏng vấn tốt hơn",
        "Sessions": "Phiên",
        "Readiness": "Mức sẵn sàng",
        "Skill Gap": "Khoảng cách kỹ năng",
        "Edit Profile": "Chỉnh sửa hồ sơ",
        "Full Name": "Họ và tên",
        "Phone": "Số điện thoại",
        "Education Level": "Trình độ học vấn",
        "Career Goal": "Mục tiêu nghề nghiệp",
        "Enter your full name": "Nhập họ và tên",
        "Email address": "Địa chỉ email",
        "Phone number": "Số điện thoại",
        "University, College, High School...": "Đại học, Cao đẳng, Trung học...",
        "Example: Become a Backend Developer specializing in cloud technologies": "Ví dụ: Trở thành Backend Developer chuyên về công nghệ đám mây",
        "Please login again!": "Vui lòng đăng nhập lại!",
        "Cannot load profile!": "Không thể tải hồ sơ!",
        "Update profile failed!": "Cập nhật hồ sơ thất bại!",
        "Session id is missing!": "Thiếu mã phiên!",
        "Cannot load answers!": "Không thể tải câu trả lời!",
        "No answers found for this session!": "Không tìm thấy câu trả lời cho phiên này!",
        "Answers evaluated successfully!": "Chấm điểm câu trả lời thành công!",
        "Cannot evaluate answers!": "Không thể đánh giá câu trả lời!",
        "Create your account": "Tạo tài khoản của bạn",
        "or with email": "hoặc bằng email",
        "I agree with the": "Tôi đồng ý với",
        "Back to home": "Quay về trang chủ",
        "Sign up with Google": "Đăng ký với Google",
        "Sign up with GitHub": "Đăng ký với GitHub",
        "Sign up with Facebook": "Đăng ký với Facebook",
        "Signing in...": "Đang đăng nhập...",
        "you@example.com": "ban@example.com",
        "Toggle navigation": "Mở/đóng menu",
        "User": "Người dùng",
        "Docker Fundamentals": "Cơ bản về Docker",
        "Container Orchestration": "Điều phối container",
        "Microservices Architecture": "Kiến trúc microservices",
        "Good explanation of Docker fundamentals": "Giải thích tốt về kiến thức Docker cơ bản",
        "Clear communication of technical concepts": "Trình bày rõ ràng các khái niệm kỹ thuật",
        "Strong practical experience demonstrated": "Thể hiện kinh nghiệm thực tế tốt",
        "Your Journey to Success": "Hành trình đến thành công",
        "Add your professional resume": "Thêm hồ sơ nghề nghiệp của bạn",
        "AI identifies missing skills": "AI xác định kỹ năng còn thiếu",
        "Practice with AI interviewer": "Luyện tập với người phỏng vấn AI",
        "Personalized skill improvement": "Cải thiện kỹ năng cá nhân hóa",
        "Simply upload your resume and let our AI analyze your current skill set": "Chỉ cần tải hồ sơ lên, AI sẽ phân tích bộ kỹ năng hiện tại của bạn",
        "Select your dream job position to identify the skills you need": "Chọn vị trí mục tiêu để xác định các kỹ năng bạn cần",
        "Our AI analyzes gaps and creates a personalized learning path": "AI phân tích khoảng cách và tạo lộ trình học tập cá nhân hóa",
        "Practice interviews and track your progress to success": "Luyện phỏng vấn và theo dõi tiến độ để đạt thành công",
        "AI-powered analysis of your resume to highlight strengths and areas for improvement": "Phân tích hồ sơ bằng AI để làm rõ điểm mạnh và điểm cần cải thiện",
        "Compare your skills against target job requirements to discover what to learn next": "So sánh kỹ năng của bạn với yêu cầu công việc để biết nên học gì tiếp theo",
        "Practice with an AI interviewer that adapts to your skill level and provides instant feedback": "Luyện tập với AI phỏng vấn thích ứng theo trình độ và đưa phản hồi tức thì",
        "Get a customized learning path that guides you from beginner to interview-ready": "Nhận lộ trình học tập riêng giúp bạn từ bước đầu đến sẵn sàng phỏng vấn",
        "Hello! I'm your AI interview assistant. Can you tell me about your experience with containerization technologies like Docker?": "Xin chào! Tôi là trợ lý phỏng vấn AI của bạn. Bạn có thể chia sẻ kinh nghiệm với công nghệ container như Docker không?",
        "I've used Docker in several projects for packaging applications. I'm familiar with creating Dockerfiles, docker-compose, and basic container management.": "Tôi đã dùng Docker trong nhiều dự án để đóng gói ứng dụng. Tôi quen với việc tạo Dockerfile, docker-compose và quản lý container cơ bản.",
        "Great! Can you explain the difference between a Docker image and a container?": "Tốt lắm! Bạn có thể giải thích sự khác nhau giữa Docker image và container không?"
    };

    Object.assign(viExtra, {
        "Enter the job description for this target job.": "Nhập mô tả công việc cho công việc mục tiêu này.",
        "Back": "Quay lại",
        "No skills extracted yet.": "Chưa trích xuất kỹ năng nào.",
        "Example: Required skills: C#, ASP.NET Core, SQL Server, REST API, JWT, Git...": "Ví dụ: Kỹ năng yêu cầu: C#, ASP.NET Core, SQL Server, REST API, JWT, Git...",
        "Required Skills:\n• C# / .NET Core\n• SQL Server\n• REST API Design\n• Docker / Kubernetes\n\nResponsibilities:\n• Design and implement APIs\n• Write clean, maintainable code\n• Collaborate with cross-functional teams": "Kỹ năng yêu cầu:\n• C# / .NET Core\n• SQL Server\n• Thiết kế REST API\n• Docker / Kubernetes\n\nTrách nhiệm:\n• Thiết kế và triển khai API\n• Viết mã sạch, dễ bảo trì\n• Phối hợp với các nhóm liên quan",
        "e.g., Backend Developer": "ví dụ: Backend Developer",
        "e.g., Software, Finance": "ví dụ: Phần mềm, Tài chính",
        "Title": "Tiêu đề",
        "Industry": "Ngành nghề",
        "Experience Level": "Cấp bậc kinh nghiệm",
        "Created": "Đã tạo",
        "Job Description Added": "Đã thêm mô tả công việc",
        "Missing Job Description": "Thiếu mô tả công việc",
        "Manage Job Description": "Quản lý mô tả công việc",
        "Ready for Analysis": "Sẵn sàng phân tích",
        "Needs Job Description": "Cần mô tả công việc",
        "Resume file preview is only available for PDF files. The file will be downloaded instead.": "Chỉ có thể xem trước tệp PDF. Tệp này sẽ được tải xuống thay thế.",
        "View Feedback": "Xem phản hồi",
        "Submit Answers": "Gửi câu trả lời",
        "Complete Interview": "Hoàn thành phỏng vấn",
        "Question Type": "Loại câu hỏi",
        "Difficulty": "Độ khó",
        "Skill Focus": "Kỹ năng trọng tâm",
        "Sample Answer": "Câu trả lời mẫu",
        "Evaluation": "Đánh giá",
        "Strengths": "Điểm mạnh",
        "Weaknesses": "Điểm yếu",
        "Improvement Suggestions": "Gợi ý cải thiện",
        "Average Score": "Điểm trung bình",
        "Question #": "Câu hỏi #",
        "Excellent": "Xuất sắc",
        "Good": "Tốt",
        "Fair": "Khá",
        "Poor": "Cần cải thiện",
        "Beginner": "Cơ bản",
        "Intermediate": "Trung bình",
        "Advanced": "Nâng cao",
        "Easy": "Dễ",
        "Hard": "Khó",
        "In progress": "Đang thực hiện",
        "Not started": "Chưa bắt đầu",
        "Complete": "Hoàn tất",
        "Incomplete": "Chưa hoàn tất",
        "View Details": "Xem chi tiết",
        "Mark Complete": "Đánh dấu hoàn thành",
        "Start Activity": "Bắt đầu hoạt động",
        "Resource": "Tài nguyên",
        "Estimated Time": "Thời gian ước tính",
        "Description": "Mô tả",
        "Objective": "Mục tiêu",
        "Learning Activities": "Hoạt động học tập",
        "Milestone": "Cột mốc",
        "Milestone Details": "Chi tiết cột mốc",
        "No recent feedback yet.": "Chưa có phản hồi gần đây.",
        "No missing skills found.": "Không tìm thấy kỹ năng còn thiếu.",
        "No roadmap progress yet.": "Chưa có tiến độ lộ trình.",
        "No interview data yet.": "Chưa có dữ liệu phỏng vấn.",
        "Unknown": "Không xác định",
        "Unknown Job": "Công việc không xác định",
        "Unknown Session": "Phiên không xác định",
        "No data available": "Chưa có dữ liệu",
        "Last updated": "Cập nhật lần cuối",
        "Today": "Hôm nay",
        "Yesterday": "Hôm qua"
    });

    Object.assign(viExtra, {
        "Target Job & JD": "Công việc mục tiêu & JD",
        "Create your target job and JD": "Tạo công việc mục tiêu và JD",
        "Create your target job and add the job description": "Tạo công việc mục tiêu và thêm mô tả công việc",
        "Practice interviews and get feedback from AI": "Luyện phỏng vấn và nhận phản hồi từ AI"
    });

    Object.assign(viExtra, {
        "Submitted": "Đã gửi",
        "Submitting...": "Đang gửi...",
        "Please select a skill gap analysis.": "Vui lòng chọn phân tích thiếu hụt kỹ năng.",
        "Please answer at least one question before submitting.": "Vui lòng trả lời ít nhất một câu hỏi trước khi gửi.",
        "Failed to submit answers. Please try again.": "Gửi câu trả lời thất bại. Vui lòng thử lại.",
        "An unexpected error occurred.": "Đã xảy ra lỗi không mong muốn.",
        "characters": "ký tự",
        "Valid": "Hợp lệ",
        "Min 10 chars": "Tối thiểu 10 ký tự",
        "more needed": "ký tự nữa",
        "Câu trả lời của bạn": "Câu trả lời của bạn"
    });

    Object.assign(vi, viExtra);

    const dictionaries = {
        en: {},
        vi
    };

    function getLanguage() {
        const saved = localStorage.getItem(STORAGE_KEY);
        return saved === "en" || saved === "vi" ? saved : DEFAULT_LANGUAGE;
    }

    function translateByPattern(text, language) {
        if (language === "en") return text;

        const patterns = [
            [/^(\d+)\s+activities$/i, "$1 hoạt động"],
            [/^Analysis #(\d+) - Score ([\d.]+)%$/i, "Phân tích #$1 - Điểm $2%"],
            [/^Session #(\d+)$/i, "Phiên #$1"],
            [/^(\d+)\/(\d+) Answered$/i, "Đã trả lời $1/$2"],
            [/^([+-]?[\d.]+)% vs previous$/i, "$1% so với lần trước"],
            [/^Required Skills:\s*$/i, "Kỹ năng yêu cầu:"],
            [/^Responsibilities:\s*$/i, "Trách nhiệm:"]
        ];

        for (const [pattern, replacement] of patterns) {
            if (pattern.test(text)) {
                return text.replace(pattern, replacement);
            }
        }

        return text;
    }

    function translate(text, language) {
        const source = String(text ?? "");
        const trimmed = source.trim();
        if (!trimmed || language === "en") return source;

        const translated = dictionaries[language]?.[trimmed];
        if (!translated) {
            const patternTranslated = translateByPattern(trimmed, language);
            return source.replace(trimmed, patternTranslated);
        }

        return source.replace(trimmed, translated);
    }

    function translateTextNode(node, language) {
        const original = node.__i18nOriginalText ?? node.nodeValue;
        node.__i18nOriginalText = original;
        node.nodeValue = translate(original, language);
    }

    function translateElementAttributes(element, language) {
        ["placeholder", "title", "aria-label"].forEach(function (attribute) {
            if (!element.hasAttribute(attribute)) return;
            const key = `__i18nOriginal_${attribute}`;
            const original = element[key] ?? element.getAttribute(attribute);
            element[key] = original;
            element.setAttribute(attribute, translate(original, language));
        });
    }

    function applyLanguage(language) {
        document.documentElement.lang = language;

        const walker = document.createTreeWalker(
            document.body,
            NodeFilter.SHOW_TEXT,
            {
                acceptNode(node) {
                    const parent = node.parentElement;
                    if (!parent) return NodeFilter.FILTER_REJECT;
                    if (["SCRIPT", "STYLE", "TEXTAREA"].includes(parent.tagName)) {
                        return NodeFilter.FILTER_REJECT;
                    }
                    return node.nodeValue.trim() ? NodeFilter.FILTER_ACCEPT : NodeFilter.FILTER_REJECT;
                }
            }
        );

        const textNodes = [];
        while (walker.nextNode()) textNodes.push(walker.currentNode);
        textNodes.forEach(node => translateTextNode(node, language));

        document.querySelectorAll("[placeholder], [title], [aria-label]").forEach(function (element) {
            translateElementAttributes(element, language);
        });

        document.querySelectorAll("[data-language-option]").forEach(function (button) {
            button.classList.toggle("active", button.dataset.languageOption === language);
        });
    }

    function setLanguage(language) {
        const normalized = language === "en" ? "en" : "vi";
        localStorage.setItem(STORAGE_KEY, normalized);
        applyLanguage(normalized);
        window.dispatchEvent(new CustomEvent("languagechange", { detail: { language: normalized } }));
    }

    function initLanguageControls() {
        document.querySelectorAll("[data-language-option]").forEach(function (button) {
            button.addEventListener("click", function () {
                setLanguage(button.dataset.languageOption);
            });
        });
    }

    document.addEventListener("DOMContentLoaded", function () {
        initLanguageControls();
        applyLanguage(getLanguage());

        const observer = new MutationObserver(function (mutations) {
            if (!mutations.some(mutation => mutation.addedNodes.length > 0)) return;
            window.requestAnimationFrame(function () {
                applyLanguage(getLanguage());
            });
        });

        observer.observe(document.body, { childList: true, subtree: true });
    });

    window.I18n = {
        getLanguage,
        setLanguage,
        applyLanguage,
        t(text) {
            return translate(text, getLanguage());
        }
    };

    window.t = window.I18n.t;
})();
