import React, { useEffect, useState } from 'react';
import ScoreCard from '../../components/Evaluation/ScoreCard';
import FeedbackCard from '../../components/Evaluation/FeedbackCard';
import { EvaluationResponse, ScoreData } from '../../types/Evaluation';

interface EvaluationResultPageProps {
    answerId?: number;
    evaluationResult?: EvaluationResponse;
}

const EvaluationResultPage: React.FC<EvaluationResultPageProps> = ({
    answerId,
    evaluationResult: propResult,
}) => {
    const [loading, setLoading] = useState<boolean>(!propResult);
    const [error, setError] = useState<string | null>(null);
    const [result, setResult] = useState<EvaluationResponse | null>(propResult || null);

    useEffect(() => {
        if (propResult) {
            setResult(propResult);
            setLoading(false);
        }
    }, [propResult]);

    const getScoreColor = (value: number): string => {
        if (value >= 80) return 'from-emerald-500 to-emerald-600';
        if (value >= 60) return 'from-amber-500 to-amber-600';
        return 'from-red-500 to-red-600';
    };

    const getScoreLabel = (value: number): string => {
        if (value >= 80) return 'Excellent';
        if (value >= 60) return 'Good';
        if (value >= 40) return 'Needs Improvement';
        return 'Poor';
    };

    const getScoreGradient = (value: number): string => {
        if (value >= 80) return 'from-emerald-100 via-white to-emerald-50';
        if (value >= 60) return 'from-amber-100 via-white to-amber-50';
        return 'from-red-100 via-white to-red-50';
    };

    if (loading) {
        return (
            <div className="min-h-screen bg-gradient-to-br from-gray-50 to-gray-100 flex items-center justify-center">
                <div className="text-center">
                    <div className="inline-block w-16 h-16 border-4 border-blue-200 border-t-blue-600 rounded-full animate-spin" />
                    <p className="mt-4 text-gray-600 font-medium">Loading evaluation...</p>
                </div>
            </div>
        );
    }

    if (error || !result) {
        return (
            <div className="min-h-screen bg-gradient-to-br from-gray-50 to-gray-100 flex items-center justify-center">
                <div className="text-center p-8 bg-white rounded-2xl shadow-lg">
                    <div className="text-5xl mb-4">😕</div>
                    <h2 className="text-xl font-semibold text-gray-800 mb-2">No Evaluation Found</h2>
                    <p className="text-gray-600">{error || 'Unable to load evaluation results.'}</p>
                </div>
            </div>
        );
    }

    const overallScoreData: ScoreData = {
        label: 'Overall Score',
        value: result.overall,
        color: 'text-gray-800',
        icon: '🏆',
    };

    const scoreCards: ScoreData[] = [
        { label: 'Clarity', value: result.clarity, color: 'text-blue-600', icon: '💎' },
        { label: 'Structure', value: result.structure, color: 'text-purple-600', icon: '🏗️' },
        { label: 'Relevance', value: result.relevance, color: 'text-teal-600', icon: '🎯' },
    ];

    return (
        <div className={`min-h-screen bg-gradient-to-br ${getScoreGradient(result.overall)} py-8 px-4`}>
            <div className="max-w-4xl mx-auto">
                {/* Header */}
                <div className="text-center mb-8">
                    <h1 className="text-3xl md:text-4xl font-bold text-gray-800 mb-2">
                        Interview Evaluation Results
                    </h1>
                    {answerId && (
                        <p className="text-gray-500 text-sm">Answer ID: {answerId}</p>
                    )}
                </div>

                {/* Overall Score Hero */}
                <div className="bg-white rounded-3xl shadow-xl p-8 mb-8 text-center">
                    <div className={`inline-flex items-center gap-2 px-4 py-2 rounded-full bg-gradient-to-r ${getScoreColor(result.overall)} text-white font-semibold mb-4`}>
                        <span>{result.overall >= 80 ? '🎉' : result.overall >= 60 ? '👍' : '💪'}</span>
                        <span>{getScoreLabel(result.overall)}</span>
                    </div>
                    
                    <div className={`text-8xl md:text-9xl font-bold bg-gradient-to-r ${getScoreColor(result.overall)} bg-clip-text text-transparent`}>
                        {result.overall.toFixed(0)}
                    </div>
                    <p className="text-gray-500 text-lg mt-2">out of 100</p>
                </div>

                {/* Score Cards Grid */}
                <div className="grid grid-cols-1 md:grid-cols-3 gap-6 mb-8">
                    {scoreCards.map((score) => (
                        <ScoreCard key={score.label} score={score} />
                    ))}
                </div>

                {/* Feedback Section */}
                <div className="grid grid-cols-1 md:grid-cols-2 gap-6 mb-8">
                    <FeedbackCard
                        title="Feedback"
                        content={result.feedback}
                        type="feedback"
                    />
                    <FeedbackCard
                        title="Improvement Suggestion"
                        content={result.improvement}
                        type="improvement"
                    />
                </div>

                {/* Action Buttons */}
                <div className="flex flex-col sm:flex-row gap-4 justify-center">
                    <button
                        onClick={() => window.history.back()}
                        className="px-8 py-3 bg-white border-2 border-gray-200 text-gray-700 font-semibold rounded-xl hover:bg-gray-50 hover:border-gray-300 transition-all duration-200 flex items-center justify-center gap-2"
                    >
                        <span>←</span>
                        <span>Back</span>
                    </button>
                    <button
                        onClick={() => window.location.reload()}
                        className="px-8 py-3 bg-gradient-to-r from-blue-600 to-blue-700 text-white font-semibold rounded-xl hover:from-blue-700 hover:to-blue-800 transition-all duration-200 shadow-lg hover:shadow-xl flex items-center justify-center gap-2"
                    >
                        <span>🔄</span>
                        <span>Try Again</span>
                    </button>
                </div>

                {/* Footer Message */}
                {result.message && (
                    <p className="text-center text-gray-500 text-sm mt-8">
                        {result.message}
                    </p>
                )}
            </div>
        </div>
    );
};

export default EvaluationResultPage;
