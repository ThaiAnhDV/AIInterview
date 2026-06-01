interface CircularProgressProps {
  value: number;
  maxValue?: number;
  size?: number;
  strokeWidth?: number;
  color?: 'blue' | 'green' | 'yellow' | 'red';
}

const colorMap = {
  blue: {
    bg: 'stroke-gray-200',
    fg: 'stroke-blue-500',
    text: 'text-blue-600',
  },
  green: {
    bg: 'stroke-gray-200',
    fg: 'stroke-green-500',
    text: 'text-green-600',
  },
  yellow: {
    bg: 'stroke-gray-200',
    fg: 'stroke-amber-500',
    text: 'text-amber-600',
  },
  red: {
    bg: 'stroke-gray-200',
    fg: 'stroke-red-500',
    text: 'text-red-600',
  },
};

export function CircularProgress({
  value,
  maxValue = 100,
  size = 120,
  strokeWidth = 10,
  color = 'blue',
}: CircularProgressProps) {
  const percentage = Math.min(Math.max((value / maxValue) * 100, 0), 100);
  const radius = (size - strokeWidth) / 2;
  const circumference = radius * 2 * Math.PI;
  const offset = circumference - (percentage / 100) * circumference;

  const getColor = (pct: number): keyof typeof colorMap => {
    if (pct >= 70) return 'green';
    if (pct >= 40) return 'yellow';
    return 'red';
  };

  const dynamicColor = color === 'blue' ? getColor(percentage) : color;
  const colors = colorMap[dynamicColor];

  return (
    <div className="relative inline-flex items-center justify-center">
      <svg width={size} height={size} className="transform -rotate-90">
        <circle
          cx={size / 2}
          cy={size / 2}
          r={radius}
          fill="none"
          strokeWidth={strokeWidth}
          className={colors.bg}
        />
        <circle
          cx={size / 2}
          cy={size / 2}
          r={radius}
          fill="none"
          strokeWidth={strokeWidth}
          strokeDasharray={circumference}
          strokeDashoffset={offset}
          strokeLinecap="round"
          className={`${colors.fg} transition-all duration-1000 ease-out`}
        />
      </svg>
      <div className="absolute inset-0 flex flex-col items-center justify-center">
        <span className={`text-2xl font-bold ${colors.text}`}>{value.toFixed(1)}</span>
        <span className="text-xs text-gray-500">/ {maxValue}</span>
      </div>
    </div>
  );
}
