import { TrendingUp, TrendingDown, Minus } from 'lucide-react';
import { CircularProgress } from './CircularProgress';
import type { ReadinessSummary } from '../../types/dashboard';

interface ReadinessScoreCardProps {
  readiness: ReadinessSummary;
}

export function ReadinessScoreCard({ readiness }: ReadinessScoreCardProps) {
  const currentScore = readiness.currentScore ?? 0;

  const getTrendIcon = () => {
    switch (readiness.trend) {
      case 'IMPROVING':
        return <TrendingUp className="w-5 h-5" />;
      case 'DECLINING':
        return <TrendingDown className="w-5 h-5" />;
      default:
        return <Minus className="w-5 h-5" />;
    }
  };

  const getTrendColor = () => {
    switch (readiness.trend) {
      case 'IMPROVING':
        return 'bg-green-100 text-green-700 border-green-200';
      case 'DECLINING':
        return 'bg-red-100 text-red-700 border-red-200';
      default:
        return 'bg-gray-100 text-gray-700 border-gray-200';
    }
  };

  const getTrendLabel = () => {
    switch (readiness.trend) {
      case 'IMPROVING':
        return 'Improving';
      case 'DECLINING':
        return 'Declining';
      case 'NEW':
        return 'New';
      default:
        return 'Stable';
    }
  };

  return (
    <div className="bg-white rounded-xl border border-gray-200 overflow-hidden">
      <div className="px-6 py-4 border-b border-gray-200 bg-gradient-to-r from-blue-500 to-cyan-500">
        <h2 className="text-lg font-semibold text-white">Readiness Score</h2>
        <p className="text-sm text-white/80">
          Based on your latest analysis
        </p>
      </div>

      <div className="p-6">
        <div className="flex items-center justify-center mb-6">
          <CircularProgress value={currentScore} maxValue={100} size={140} strokeWidth={12} />
        </div>

        <div className="space-y-4">
          <div className={`inline-flex items-center gap-2 px-4 py-2 rounded-lg border ${getTrendColor()}`}>
            {getTrendIcon()}
            <span className="font-medium">{getTrendLabel()}</span>
            {readiness.improvementPercentage !== 0 && (
              <span className="text-sm">
                ({readiness.improvementPercentage > 0 ? '+' : ''}{readiness.improvementPercentage.toFixed(1)}%)
              </span>
            )}
          </div>

          <div className="grid grid-cols-2 gap-4 pt-4 border-t border-gray-100">
            <div className="text-center">
              <p className="text-sm text-gray-500 mb-1">Current</p>
              <p className="text-xl font-bold text-gray-900">
                {currentScore.toFixed(1)}
              </p>
            </div>
            <div className="text-center">
              <p className="text-sm text-gray-500 mb-1">Previous</p>
              <p className="text-xl font-bold text-gray-500">
                {readiness.previousScore?.toFixed(1) ?? '-'}
              </p>
            </div>
          </div>

          {readiness.calculatedAt && (
            <p className="text-center text-sm text-gray-400 pt-2">
              Last updated: {new Date(readiness.calculatedAt).toLocaleDateString('en-US', {
                month: 'short',
                day: 'numeric',
                year: 'numeric'
              })}
            </p>
          )}
        </div>
      </div>
    </div>
  );
}
