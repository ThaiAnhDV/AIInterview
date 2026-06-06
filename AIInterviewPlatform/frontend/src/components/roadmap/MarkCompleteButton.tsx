import { useState } from 'react';
import { CheckCircle, Loader2, AlertCircle } from 'lucide-react';
import { useCompleteActivity } from '../../hooks/useRoadmap';

interface MarkCompleteButtonProps {
  activityId: number;
  activityTitle: string;
  onCompleted?: () => void;
}

export function MarkCompleteButton({ activityId, activityTitle, onCompleted }: MarkCompleteButtonProps) {
  const [showConfirm, setShowConfirm] = useState(false);
  const completeActivity = useCompleteActivity();

  const handleComplete = async () => {
    try {
      await completeActivity.mutateAsync(activityId);
      setShowConfirm(false);
      onCompleted?.();
    } catch (error) {
      console.error('Failed to complete activity:', error);
    }
  };

  if (completeActivity.isSuccess) {
    return (
      <div className="flex items-center gap-2 text-green-600">
        <CheckCircle className="w-4 h-4" />
        <span className="text-sm font-medium">Completed</span>
      </div>
    );
  }

  if (showConfirm) {
    return (
      <div className="flex items-center gap-2">
        <span className="text-sm text-gray-600">Mark complete?</span>
        <button
          onClick={handleComplete}
          disabled={completeActivity.isPending}
          className="px-3 py-1.5 text-sm font-medium text-white bg-green-600 rounded-lg hover:bg-green-700 disabled:opacity-50 transition-colors"
        >
          {completeActivity.isPending ? (
            <Loader2 className="w-4 h-4 animate-spin" />
          ) : (
            'Confirm'
          )}
        </button>
        <button
          onClick={() => setShowConfirm(false)}
          className="px-3 py-1.5 text-sm font-medium text-gray-600 bg-gray-100 rounded-lg hover:bg-gray-200 transition-colors"
        >
          Cancel
        </button>
      </div>
    );
  }

  return (
    <button
      onClick={() => setShowConfirm(true)}
      className="inline-flex items-center gap-1.5 px-3 py-1.5 text-sm font-medium text-green-600 bg-green-50 border border-green-200 rounded-lg hover:bg-green-100 transition-colors"
    >
      <CheckCircle className="w-4 h-4" />
      Mark Complete
    </button>
  );
}
