import { useMutation, useQueryClient } from '@tanstack/react-query';
import { roadmapApi } from '../services/roadmapApi';
import { DASHBOARD_KEY } from './useDashboard';

export function useCompleteActivity() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (activityId: number) => roadmapApi.completeActivity(activityId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: DASHBOARD_KEY });
    },
  });
}
