import { useQuery } from '@tanstack/react-query';
import { dashboardApi } from '../services/dashboardApi';
import type { DashboardDto } from '../types/dashboard';

export const DASHBOARD_KEY = ['dashboard'] as const;

interface UseDashboardOptions {
  refetchInterval?: number;
  staleTime?: number;
}

export function useDashboard(options: UseDashboardOptions = {}) {
  const { refetchInterval, staleTime = 5 * 60 * 1000 } = options;

  return useQuery<DashboardDto, Error>({
    queryKey: DASHBOARD_KEY,
    queryFn: () => dashboardApi.getDashboard(),
    staleTime,
    refetchInterval,
    retry: 2,
    retryDelay: (attemptIndex) => Math.min(1000 * 2 ** attemptIndex, 30000),
  });
}

export function useDashboardRefresh() {
  return useQuery({
    queryKey: DASHBOARD_KEY,
    queryFn: () => dashboardApi.getDashboard(),
    staleTime: 0,
  });
}
