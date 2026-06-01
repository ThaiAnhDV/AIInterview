import { useCallback } from 'react';
import { Loader2, AlertCircle, RefreshCw, Target, TrendingUp, CheckCircle, MessageSquare } from 'lucide-react';
import { useQueryClient } from '@tanstack/react-query';
import { useDashboard, DASHBOARD_KEY } from '../../hooks/useDashboard';
import type { DashboardDto } from '../../types/dashboard';
import { ReadinessScoreCard } from './ReadinessScoreCard';
import { SkillGapVisualization } from './SkillGapVisualization';
import { RecentFeedbackSection } from './RecentFeedbackSection';
import { RoadmapProgressSection } from './RoadmapProgressSection';

export default function Dashboard() {
  const queryClient = useQueryClient();
  const { data: dashboardData, isLoading, error, isFetching } = useDashboard();

  const handleRefresh = useCallback(async () => {
    await queryClient.invalidateQueries({ queryKey: DASHBOARD_KEY });
  }, [queryClient]);

  if (isLoading) {
    return (
      <div className="flex flex-col items-center justify-center min-h-[400px]">
        <Loader2 className="w-12 h-12 text-primary-500 animate-spin mb-4" />
        <p className="text-gray-500 font-medium">Loading dashboard...</p>
      </div>
    );
  }

  if (error) {
    return (
      <div className="flex flex-col items-center justify-center min-h-[400px]">
        <div className="bg-white border border-red-200 rounded-2xl p-8 text-center max-w-md shadow-sm">
          <div className="w-16 h-16 bg-red-100 rounded-full flex items-center justify-center mx-auto mb-4">
            <AlertCircle className="w-8 h-8 text-red-500" />
          </div>
          <h3 className="text-lg font-semibold text-gray-900 mb-2">Error Loading Dashboard</h3>
          <p className="text-red-600 mb-2">{error.message}</p>
          <p className="text-sm text-gray-500 mb-6">Please check your connection and try again.</p>
          <button
            onClick={handleRefresh}
            className="inline-flex items-center gap-2 px-6 py-3 bg-red-600 text-white font-medium rounded-lg hover:bg-red-700 transition-colors"
          >
            <RefreshCw className="w-4 h-4" />
            Try Again
          </button>
        </div>
      </div>
    );
  }

  if (!dashboardData) {
    return null;
  }

  return (
    <div className="space-y-6">
      <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">Dashboard</h1>
          <p className="text-gray-500 mt-1">
            Your interview preparation overview
          </p>
        </div>
        <button
          onClick={handleRefresh}
          disabled={isFetching}
          className="inline-flex items-center gap-2 px-4 py-2 text-sm font-medium text-gray-600 hover:text-gray-900 bg-white border border-gray-200 rounded-lg hover:bg-gray-50 transition-colors disabled:opacity-50"
        >
          <RefreshCw className={`w-4 h-4 ${isFetching ? 'animate-spin' : ''}`} />
          {isFetching ? 'Refreshing...' : 'Refresh'}
        </button>
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
        <div className="lg:col-span-2 space-y-6">
          <div className="grid grid-cols-2 lg:grid-cols-4 gap-4">
            <div className="col-span-2 lg:col-span-1">
              <div className="bg-white rounded-xl border border-gray-200 p-4">
                <div className="flex items-center gap-3">
                  <div className="p-2 bg-blue-50 rounded-lg">
                    <Target className="w-5 h-5 text-blue-600" />
                  </div>
                  <div>
                    <p className="text-2xl font-bold text-gray-900">
                      {dashboardData.readiness.currentScore?.toFixed(0) ?? '0'}
                    </p>
                    <p className="text-xs text-gray-500">Readiness</p>
                  </div>
                </div>
              </div>
            </div>

            <div className="col-span-2 lg:col-span-1">
              <div className="bg-white rounded-xl border border-gray-200 p-4">
                <div className="flex items-center gap-3">
                  <div className="p-2 bg-orange-50 rounded-lg">
                    <TrendingUp className="w-5 h-5 text-orange-600" />
                  </div>
                  <div>
                    <p className="text-2xl font-bold text-gray-900">
                      {dashboardData.skillGaps.totalMissingSkills}
                    </p>
                    <p className="text-xs text-gray-500">Missing Skills</p>
                  </div>
                </div>
              </div>
            </div>

            <div className="col-span-2 lg:col-span-1">
              <div className="bg-white rounded-xl border border-gray-200 p-4">
                <div className="flex items-center gap-3">
                  <div className="p-2 bg-green-50 rounded-lg">
                    <CheckCircle className="w-5 h-5 text-green-600" />
                  </div>
                  <div>
                    <p className="text-2xl font-bold text-gray-900">
                      {dashboardData.interviews.totalInterviews}
                    </p>
                    <p className="text-xs text-gray-500">Interviews</p>
                  </div>
                </div>
              </div>
            </div>

            <div className="col-span-2 lg:col-span-1">
              <div className="bg-white rounded-xl border border-gray-200 p-4">
                <div className="flex items-center gap-3">
                  <div className="p-2 bg-purple-50 rounded-lg">
                    <MessageSquare className="w-5 h-5 text-purple-600" />
                  </div>
                  <div>
                    <p className="text-2xl font-bold text-gray-900">
                      {dashboardData.interviews.averageScore.toFixed(1)}
                    </p>
                    <p className="text-xs text-gray-500">Avg Score</p>
                  </div>
                </div>
              </div>
            </div>
          </div>

          <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
            <ReadinessScoreCard readiness={dashboardData.readiness} />
            <RoadmapProgressSection progress={dashboardData.roadmapProgress} />
          </div>

          <SkillGapVisualization skills={dashboardData.skillGaps.missingSkills} />
        </div>

        <div className="space-y-6">
          <div className="bg-white rounded-xl border border-gray-200 overflow-hidden">
            <div className="px-6 py-4 border-b border-gray-200 bg-gray-50">
              <h3 className="font-semibold text-gray-900">Priority Skills</h3>
              <p className="text-sm text-gray-500">Focus areas</p>
            </div>
            <div className="divide-y divide-gray-100">
              {dashboardData.skillGaps.missingSkills.slice(0, 5).map((skill) => (
                <div key={skill.skillId} className="p-4 hover:bg-gray-50 transition-colors">
                  <div className="flex items-center justify-between">
                    <div>
                      <p className="font-medium text-gray-900">{skill.skillName}</p>
                      {skill.skillType && (
                        <p className="text-xs text-gray-500">{skill.skillType}</p>
                      )}
                    </div>
                    <span className={`px-2 py-1 rounded-full text-xs font-medium text-white ${
                      skill.priority <= 2 ? 'bg-red-500' : skill.priority <= 3 ? 'bg-amber-500' : 'bg-blue-500'
                    }`}>
                      P{skill.priority}
                    </span>
                  </div>
                </div>
              ))}
              {dashboardData.skillGaps.missingSkills.length === 0 && (
                <div className="p-8 text-center text-gray-500">
                  No missing skills
                </div>
              )}
            </div>
          </div>

          <RecentFeedbackSection feedbacks={dashboardData.recentFeedbacks} />
        </div>
      </div>
    </div>
  );
}
