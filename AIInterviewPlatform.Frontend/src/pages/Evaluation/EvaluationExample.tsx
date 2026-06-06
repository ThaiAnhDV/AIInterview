import React, { useState } from 'react';
import EvaluationResultPage from './EvaluationResultPage';
import { useEvaluation } from '../../hooks/useEvaluation';
import { EvaluationResponse } from '../../types/Evaluation';

const EvaluationExample: React.FC = () => {
    const [answerId, setAnswerId] = useState<number>(0);
    const [showResult, setShowResult] = useState<boolean>(false);
    const [result, setResult] = useState<EvaluationResponse | null>(null);

    const { evaluate, loading, error } = useEvaluation({
        onSuccess: (data) => {
            setResult(data);
            setShowResult(true);
        },
    });

    const handleEvaluate = async () => {
        if (answerId <= 0) return;
        await evaluate(answerId);
    };

    if (showResult && result) {
        return (
            <EvaluationResultPage
                answerId={answerId}
                evaluationResult={result}
            />
        );
    }

    return (
        <div className="min-h-screen bg-gray-100 flex items-center justify-center p-4">
            <div className="bg-white rounded-2xl shadow-xl p-8 max-w-md w-full">
                <h1 className="text-2xl font-bold text-gray-800 mb-6 text-center">
                    Evaluate Interview Answer
                </h1>

                <div className="mb-6">
                    <label className="block text-sm font-medium text-gray-700 mb-2">
                        Answer ID
                    </label>
                    <input
                        type="number"
                        value={answerId || ''}
                        onChange={(e) => setAnswerId(parseInt(e.target.value) || 0)}
                        placeholder="Enter answer ID"
                        className="w-full px-4 py-3 border-2 border-gray-200 rounded-xl focus:border-blue-500 focus:outline-none transition-colors"
                    />
                </div>

                {error && (
                    <div className="mb-4 p-4 bg-red-50 border border-red-200 rounded-xl text-red-700 text-sm">
                        {error}
                    </div>
                )}

                <button
                    onClick={handleEvaluate}
                    disabled={loading || answerId <= 0}
                    className="w-full py-3 bg-blue-600 text-white font-semibold rounded-xl hover:bg-blue-700 disabled:bg-gray-300 disabled:cursor-not-allowed transition-all duration-200"
                >
                    {loading ? (
                        <span className="flex items-center justify-center gap-2">
                            <span className="w-5 h-5 border-2 border-white border-t-transparent rounded-full animate-spin" />
                            Evaluating...
                        </span>
                    ) : (
                        'Evaluate Answer'
                    )}
                </button>
            </div>
        </div>
    );
};

export default EvaluationExample;
