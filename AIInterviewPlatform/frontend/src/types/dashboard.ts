export interface ReadinessSummary {
  currentScore: number | null;
  previousScore: number | null;
  improvementPercentage: number;
  trend: string;
  calculatedAt: string | null;
}

export interface MissingSkill {
  skillId: number;
  skillName: string;
  skillType: string | null;
  gapLevel: string;
  gapDescription: string | null;
  priority: number;
}

export interface SkillGapSummary {
  totalMissingSkills: number;
  highPriorityCount: number;
  mediumPriorityCount: number;
  lowPriorityCount: number;
  missingSkills: MissingSkill[];
  lastAnalyzedAt: string | null;
}

export interface InterviewSummary {
  totalInterviews: number;
  completedInterviews: number;
  pendingInterviews: number;
  averageScore: number;
  highestScore: number | null;
  lowestScore: number | null;
}

export interface RoadmapProgress {
  totalRoadmaps: number;
  overallProgressPercentage: number;
  completedMilestones: number;
  totalMilestones: number;
  activeRoadmapTitle: string | null;
  activeRoadmapProgress: number | null;
}

export interface RecentFeedback {
  feedbackId: number;
  interviewAnswerId: number;
  feedbackType: string;
  content: string;
  score: number | null;
  createdAt: string;
}

export interface DashboardDto {
  readiness: ReadinessSummary;
  skillGaps: SkillGapSummary;
  interviews: InterviewSummary;
  roadmapProgress: RoadmapProgress;
  recentFeedbacks: RecentFeedback[];
  generatedAt: string;
}
