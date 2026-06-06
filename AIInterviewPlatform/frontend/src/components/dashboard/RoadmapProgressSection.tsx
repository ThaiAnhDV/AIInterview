import { Map, CheckCircle, Clock } from 'lucide-react';
import type { RoadmapProgress } from '../../types/dashboard';

interface RoadmapProgressSectionProps {
  progress: RoadmapProgress | null;
}

export function RoadmapProgressSection({ progress }: RoadmapProgressSectionProps) {
  if (!progress) {
    return (
      <div className="bg-white rounded-xl border border-gray-200 p-6">
        <h2 className="text-lg font-semibold text-gray-900 mb-4">Roadmap Progress</h2>
        <div className="text-center py-8 text-gray-500">
          <Map className="w-12 h-12 mx-auto mb-3 text-gray-300" />
          <p>No roadmaps assigned yet</p>
        </div>
      </div>
    );
  }

  const milestonePercentage = progress.totalMilestones > 0
    ? (progress.completedMilestones / progress.totalMilestones) * 100
    : 0;

  return (
    <div className="bg-white rounded-xl border border-gray-200 overflow-hidden">
      <div className="px-6 py-4 border-b border-gray-200 bg-gradient-to-r from-indigo-500 to-purple-500">
        <div className="flex items-center justify-between">
          <div className="flex items-center gap-3">
            <div className="p-2 bg-white/20 rounded-lg">
              <Map className="w-5 h-5 text-white" />
            </div>
            <div>
              <h2 className="text-lg font-semibold text-white">Learning Roadmap</h2>
              {progress.activeRoadmapTitle && (
                <p className="text-sm text-white/80">{progress.activeRoadmapTitle}</p>
              )}
            </div>
          </div>
          <div className="text-right">
            <p className="text-2xl font-bold text-white">
              {progress.overallProgressPercentage.toFixed(0)}%
            </p>
            <p className="text-xs text-white/70">Complete</p>
          </div>
        </div>
      </div>

      <div className="p-6 space-y-6">
        <div>
          <div className="flex items-center justify-between text-sm mb-2">
            <span className="text-gray-600 font-medium">Overall Progress</span>
            <span className="text-gray-900 font-semibold">
              {progress.overallProgressPercentage.toFixed(1)}%
            </span>
          </div>
          <div className="h-4 bg-gray-100 rounded-full overflow-hidden">
            <div
              className="h-full bg-gradient-to-r from-indigo-500 to-purple-500 rounded-full transition-all duration-1000 ease-out relative"
              style={{ width: `${progress.overallProgressPercentage}%` }}
            >
              <div className="absolute inset-0 bg-white/20 animate-pulse" />
            </div>
          </div>
        </div>

        <div className="grid grid-cols-3 gap-4">
          <div className="bg-gray-50 rounded-lg p-4 text-center">
            <div className="flex items-center justify-center gap-2 text-gray-600 mb-1">
              <Map className="w-4 h-4" />
              <span className="text-xs font-medium">Roadmaps</span>
            </div>
            <p className="text-2xl font-bold text-gray-900">{progress.totalRoadmaps}</p>
          </div>

          <div className="bg-green-50 rounded-lg p-4 text-center">
            <div className="flex items-center justify-center gap-2 text-green-600 mb-1">
              <CheckCircle className="w-4 h-4" />
              <span className="text-xs font-medium">Completed</span>
            </div>
            <p className="text-2xl font-bold text-green-600">{progress.completedMilestones}</p>
          </div>

          <div className="bg-amber-50 rounded-lg p-4 text-center">
            <div className="flex items-center justify-center gap-2 text-amber-600 mb-1">
              <Clock className="w-4 h-4" />
              <span className="text-xs font-medium">Remaining</span>
            </div>
            <p className="text-2xl font-bold text-amber-600">
              {progress.totalMilestones - progress.completedMilestones}
            </p>
          </div>
        </div>

        <div>
          <div className="flex items-center justify-between text-sm mb-2">
            <span className="text-gray-600 font-medium">Milestone Progress</span>
            <span className="text-gray-900 font-semibold">
              {progress.completedMilestones} / {progress.totalMilestones}
            </span>
          </div>
          <div className="h-2 bg-gray-100 rounded-full overflow-hidden">
            <div
              className="h-full bg-green-500 rounded-full transition-all duration-1000 ease-out"
              style={{ width: `${milestonePercentage}%` }}
            />
          </div>
        </div>

        {progress.activeRoadmapProgress !== null && (
          <div className="pt-4 border-t border-gray-100">
            <div className="flex items-center justify-between text-sm mb-2">
              <span className="text-gray-600 font-medium">Active Roadmap</span>
              <span className="text-indigo-600 font-semibold">
                {progress.activeRoadmapProgress.toFixed(1)}%
              </span>
            </div>
            <div className="h-2 bg-gray-100 rounded-full overflow-hidden">
              <div
                className="h-full bg-indigo-500 rounded-full transition-all duration-1000 ease-out"
                style={{ width: `${progress.activeRoadmapProgress}%` }}
              />
            </div>
          </div>
        )}
      </div>
    </div>
  );
}
