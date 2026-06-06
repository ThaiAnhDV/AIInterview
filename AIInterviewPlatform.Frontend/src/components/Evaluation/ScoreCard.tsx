import React from 'react';
import { ScoreData } from '../../types/Evaluation';

interface ScoreCardProps {
    score: ScoreData;
    isLarge?: boolean;
}

const ScoreCard: React.FC<ScoreCardProps> = ({ score, isLarge = false }) => {
    const getScoreColor = (value: number): string => {
        if (value >= 80) return 'text-emerald-500';
        if (value >= 60) return 'text-amber-500';
        return 'text-red-500';
    };

    const getBgColor = (value: number): string => {
        if (value >= 80) return 'bg-emerald-50 border-emerald-200';
        if (value >= 60) return 'bg-amber-50 border-amber-200';
        return 'bg-red-50 border-red-200';
    };

    const getProgressColor = (value: number): string => {
        if (value >= 80) return 'bg-emerald-500';
        if (value >= 60) return 'bg-amber-500';
        return 'bg-red-500';
    };

    return (
        <div
            className={`rounded-2xl border-2 p-6 transition-all duration-300 hover:shadow-lg ${
                getBgColor(score.value)
            } ${isLarge ? 'md:p-8' : ''}`}
        >
            <div className="flex items-center justify-between mb-4">
                <div className="flex items-center gap-3">
                    <span className="text-2xl">{score.icon}</span>
                    <span className="text-sm font-medium text-gray-600 uppercase tracking-wide">
                        {score.label}
                    </span>
                </div>
            </div>

            <div className="flex items-end justify-between">
                <div className={`font-bold ${isLarge ? 'text-5xl md:text-6xl' : 'text-3xl md:text-4xl'} ${getScoreColor(score.value)}`}>
                    {score.value.toFixed(0)}
                    <span className="text-2xl md:text-3xl text-gray-400">/100</span>
                </div>
            </div>

            <div className="mt-4 h-2 bg-gray-200 rounded-full overflow-hidden">
                <div
                    className={`h-full rounded-full transition-all duration-1000 ease-out ${getProgressColor(score.value)}`}
                    style={{ width: `${score.value}%` }}
                />
            </div>
        </div>
    );
};

export default ScoreCard;
