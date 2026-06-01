import axios from 'axios';
import type { DashboardDto } from '../types/dashboard';

const API_BASE_URL = import.meta.env.VITE_API_URL || 'http://localhost:5000/api';

const apiClient = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    'Content-Type': 'application/json',
  },
});

apiClient.interceptors.request.use((config) => {
  const token = localStorage.getItem('authToken');
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

export const dashboardApi = {
  getDashboard: async (): Promise<DashboardDto> => {
    const response = await apiClient.get<DashboardDto>('/dashboard');
    return response.data;
  },
};

export interface ActivityCompletionResponse {
  success: boolean;
  message: string;
  data?: {
    activityId: number;
    milestoneId: number;
    milestoneCompleted: boolean;
    milestoneProgress: number;
    roadmapProgress: number;
  };
}

export const roadmapApi = {
  completeActivity: async (activityId: number): Promise<ActivityCompletionResponse> => {
    const response = await apiClient.post<ActivityCompletionResponse>(
      `/Roadmap/complete-activity/${activityId}`
    );
    return response.data;
  },
};

export default apiClient;
