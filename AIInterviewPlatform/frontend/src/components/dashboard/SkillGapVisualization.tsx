import { AlertCircle, AlertTriangle, Info } from 'lucide-react';
import type { MissingSkill } from '../../types/dashboard';

interface SkillGapVisualizationProps {
  skills: MissingSkill[];
}

const priorityConfig = {
  1: { label: 'Critical', color: 'bg-red-500', icon: AlertCircle },
  2: { label: 'High', color: 'bg-orange-500', icon: AlertTriangle },
  3: { label: 'Medium', color: 'bg-yellow-500', icon: Info },
  4: { label: 'Low', color: 'bg-blue-500', icon: Info },
};

export function SkillGapVisualization({ skills }: SkillGapVisualizationProps) {
  const sortedSkills = [...skills].sort((a, b) => a.priority - b.priority);
  const topSkills = sortedSkills.slice(0, 5);

  const maxPriority = 4;
  const getBarWidth = (priority: number) => {
    return ((maxPriority - priority + 1) / maxPriority) * 100;
  };

  if (skills.length === 0) {
    return (
      <div className="bg-white rounded-xl border border-gray-200 p-6">
        <h2 className="text-lg font-semibold text-gray-900 mb-4">Skill Gap Analysis</h2>
        <div className="text-center py-8 text-gray-500">
          <p>No skill gaps identified. Great job!</p>
        </div>
      </div>
    );
  }

  return (
    <div className="bg-white rounded-xl border border-gray-200 overflow-hidden">
      <div className="px-6 py-4 border-b border-gray-200 bg-gray-50">
        <h2 className="text-lg font-semibold text-gray-900">Skill Gap Analysis</h2>
        <p className="text-sm text-gray-500 mt-1">
          Top {topSkills.length} skills to focus on
        </p>
      </div>

      <div className="p-6 space-y-4">
        {topSkills.map((skill, index) => {
          const config = priorityConfig[skill.priority as keyof typeof priorityConfig] || priorityConfig[4];
          const Icon = config.icon;
          const barWidth = getBarWidth(skill.priority);

          return (
            <div key={skill.skillId} className="space-y-2">
              <div className="flex items-center justify-between">
                <div className="flex items-center gap-2">
                  <span className="text-xs font-medium text-gray-400 w-5">
                    #{index + 1}
                  </span>
                  <h4 className="font-medium text-gray-900">{skill.skillName}</h4>
                  {skill.skillType && (
                    <span className="px-2 py-0.5 text-xs rounded-full bg-gray-100 text-gray-600">
                      {skill.skillType}
                    </span>
                  )}
                </div>
                <div className="flex items-center gap-2">
                  <span className={`inline-flex items-center gap-1 px-2 py-0.5 rounded-full text-xs font-medium text-white ${config.color}`}>
                    <Icon className="w-3 h-3" />
                    {config.label}
                  </span>
                </div>
              </div>
              <div className="relative">
                <div className="h-2 bg-gray-100 rounded-full overflow-hidden">
                  <div
                    className={`h-full rounded-full transition-all duration-700 ease-out ${config.color}`}
                    style={{ width: `${barWidth}%` }}
                  />
                </div>
                <span className="absolute right-0 -top-5 text-xs text-gray-500">
                  Priority {skill.priority}
                </span>
              </div>
            </div>
          );
        })}
      </div>

      {skills.length > 5 && (
        <div className="px-6 py-3 border-t border-gray-100 bg-gray-50 text-center">
          <p className="text-sm text-gray-500">
            +{skills.length - 5} more skills to improve
          </p>
        </div>
      )}
    </div>
  );
}
