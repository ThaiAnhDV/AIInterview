import React from 'react';

interface FeedbackCardProps {
    title: string;
    content: string;
    type: 'feedback' | 'improvement';
}

const FeedbackCard: React.FC<FeedbackCardProps> = ({ title, content, type }) => {
    const isImprovement = type === 'improvement';

    return (
        <div
            className={`rounded-2xl border-2 p-6 ${
                isImprovement
                    ? 'bg-indigo-50 border-indigo-200'
                    : 'bg-blue-50 border-blue-200'
            }`}
        >
            <div className="flex items-center gap-3 mb-4">
                <div
                    className={`w-10 h-10 rounded-full flex items-center justify-center ${
                        isImprovement ? 'bg-indigo-200' : 'bg-blue-200'
                    }`}
                >
                    <span className="text-xl">
                        {isImprovement ? '💡' : '💬'}
                    </span>
                </div>
                <h3
                    className={`text-lg font-semibold ${
                        isImprovement ? 'text-indigo-900' : 'text-blue-900'
                    }`}
                >
                    {title}
                </h3>
            </div>

            <p
                className={`text-base leading-relaxed ${
                    isImprovement ? 'text-indigo-800' : 'text-blue-800'
                }`}
            >
                {content}
            </p>
        </div>
    );
};

export default FeedbackCard;
