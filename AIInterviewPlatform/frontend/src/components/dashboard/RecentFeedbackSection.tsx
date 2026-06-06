import { MessageSquare, Star, Clock } from 'lucide-react';
import type { RecentFeedback } from '../../types/dashboard';

interface RecentFeedbackSectionProps {
  feedbacks: RecentFeedback[];
}

const feedbackTypeColors: Record<string, { bg: string; text: string }> = {
  IMPROVEMENT: { bg: 'bg-amber-50', text: 'text-amber-700' },
  STRENGTH: { bg: 'bg-emerald-50', text: 'text-emerald-700' },
  TIP: { bg: 'bg-blue-50', text: 'text-blue-700' },
  WARNING: { bg: 'bg-red-50', text: 'text-red-700' },
};

export function RecentFeedbackSection({ feedbacks }: RecentFeedbackSectionProps) {
  if (feedbacks.length === 0) {
    return (
      <div className="bg-white rounded-xl border border-gray-200 p-6">
        <h2 className="text-lg font-semibold text-gray-900 mb-4">Recent Feedback</h2>
        <div className="text-center py-8 text-gray-500">
          <p>No recent feedback available.</p>
        </div>
      </div>
    );
  }

  return (
    <div className="bg-white rounded-xl border border-gray-200 overflow-hidden">
      <div className="px-6 py-4 border-b border-gray-200 bg-gray-50">
        <h2 className="text-lg font-semibold text-gray-900">Recent Feedback</h2>
        <p className="text-sm text-gray-500 mt-1">
          Latest feedback from your interview sessions
        </p>
      </div>
      
      <div className="divide-y divide-gray-100 max-h-96 overflow-y-auto">
        {feedbacks.map((feedback) => {
          const colors = feedbackTypeColors[feedback.feedbackType] || { bg: 'bg-gray-50', text: 'text-gray-700' };
          
          return (
            <div key={feedback.feedbackId} className="p-4 hover:bg-gray-50 transition-colors">
              <div className="flex items-start gap-3">
                <div className={`p-2 rounded-lg ${colors.bg}`}>
                  <MessageSquare className={`w-5 h-5 ${colors.text}`} />
                </div>
                
                <div className="flex-1 min-w-0">
                  <div className="flex items-center justify-between gap-2 mb-1">
                    <span className={`inline-flex items-center px-2 py-0.5 rounded-full text-xs font-medium ${colors.bg} ${colors.text}`}>
                      {feedback.feedbackType}
                    </span>
                    
                    <div className="flex items-center gap-3 text-xs text-gray-500">
                      {feedback.score !== null && (
                        <span className="flex items-center gap-1">
                          <Star className="w-3 h-3 text-amber-500" />
                          {feedback.score.toFixed(1)}
                        </span>
                      )}
                      <span className="flex items-center gap-1">
                        <Clock className="w-3 h-3" />
                        {new Date(feedback.createdAt).toLocaleDateString()}
                      </span>
                    </div>
                  </div>
                  
                  <p className="text-sm text-gray-700 line-clamp-3">{feedback.content}</p>
                </div>
              </div>
            </div>
          );
        })}
      </div>
    </div>
  );
}
