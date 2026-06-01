export interface EvaluationResponse {
    answerId: number;
    clarity: number;
    structure: number;
    relevance: number;
    overall: number;
    feedback: string;
    improvement: string;
    message: string;
}

export interface EvaluationRequest {
    answerId: number;
}

export type ScoreCategory = 'clarity' | 'structure' | 'relevance' | 'overall';

export interface ScoreData {
    label: string;
    value: number;
    color: string;
    icon: string;
}
