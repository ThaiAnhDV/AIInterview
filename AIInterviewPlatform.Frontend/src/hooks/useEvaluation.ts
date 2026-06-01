import { useState, useCallback } from 'react';
import { EvaluationResponse, EvaluationRequest } from '../types/Evaluation';

const API_BASE_URL = '/api/interviews';

interface UseEvaluationOptions {
    onSuccess?: (result: EvaluationResponse) => void;
    onError?: (error: string) => void;
}

interface UseEvaluationReturn {
    evaluate: (answerId: number) => Promise<EvaluationResponse | null>;
    loading: boolean;
    error: string | null;
    result: EvaluationResponse | null;
    reset: () => void;
}

export const useEvaluation = (options?: UseEvaluationOptions): UseEvaluationReturn => {
    const [loading, setLoading] = useState<boolean>(false);
    const [error, setError] = useState<string | null>(null);
    const [result, setResult] = useState<EvaluationResponse | null>(null);

    const evaluate = useCallback(async (answerId: number): Promise<EvaluationResponse | null> => {
        setLoading(true);
        setError(null);

        try {
            const request: EvaluationRequest = { answerId };

            const response = await fetch(`${API_BASE_URL}/evaluate`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                credentials: 'include',
                body: JSON.stringify(request),
            });

            if (!response.ok) {
                const errorData = await response.json().catch(() => ({}));
                const errorMessage = errorData.detail || `Request failed with status ${response.status}`;
                throw new Error(errorMessage);
            }

            const data: EvaluationResponse = await response.json();
            setResult(data);
            options?.onSuccess?.(data);
            return data;
        } catch (err) {
            const errorMessage = err instanceof Error ? err.message : 'An unexpected error occurred';
            setError(errorMessage);
            options?.onError?.(errorMessage);
            return null;
        } finally {
            setLoading(false);
        }
    }, [options]);

    const reset = useCallback(() => {
        setLoading(false);
        setError(null);
        setResult(null);
    }, []);

    return { evaluate, loading, error, result, reset };
};

export default useEvaluation;
