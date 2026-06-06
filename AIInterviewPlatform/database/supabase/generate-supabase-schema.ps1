$sourcePath = 'D:\AIInterview - Copy - Copy\AIinteview.sql'
$outPath = Join-Path $PSScriptRoot 'schema.sql'
$content = Get-Content -LiteralPath $sourcePath -Raw

$startMarker = "-- =========================================================`r`n-- 2. USERS & ACCOUNT MANAGEMENT"
$start = $content.IndexOf($startMarker)
if ($start -lt 0) {
    $startMarker = "-- =========================================================`n-- 2. USERS & ACCOUNT MANAGEMENT"
    $start = $content.IndexOf($startMarker)
}
if ($start -lt 0) {
    throw 'Cannot find schema start marker.'
}

$content = $content.Substring($start)

$verifyMarker = "-- =========================================================`r`n-- 14. VERIFY TABLES"
$verify = $content.IndexOf($verifyMarker)
if ($verify -lt 0) {
    $verifyMarker = "-- =========================================================`n-- 14. VERIFY TABLES"
    $verify = $content.IndexOf($verifyMarker)
}
if ($verify -ge 0) {
    $content = $content.Substring(0, $verify)
}

$content = $content -replace '(?m)^GO\s*$', ';'
$content = $content -replace 'BIGINT\s+IDENTITY\(1,1\)\s+PRIMARY KEY', 'BIGSERIAL PRIMARY KEY'
$content = $content -replace 'NVARCHAR\(MAX\)', 'TEXT'
$content = $content -replace 'NVARCHAR\((\d+)\)', 'VARCHAR($1)'
$content = $content -replace 'DATETIME2', 'TIMESTAMP'
$content = $content -replace 'GETDATE\(\)', 'CURRENT_TIMESTAMP'
$content = $content -replace 'BIT\s+NOT NULL\s+DEFAULT\s+0', 'BOOLEAN NOT NULL DEFAULT FALSE'
$content = $content -replace 'BIT\s+NOT NULL\s+DEFAULT\s+1', 'BOOLEAN NOT NULL DEFAULT TRUE'
$content = $content -replace 'DECIMAL\(', 'NUMERIC('

$content = $content -replace '(?m)^\s*DECLARE @AdminUserId BIGINT;\s*\r?\n\s*SET @AdminUserId = SCOPE_IDENTITY\(\);\s*\r?\n', ''
$content = $content -replace "(\s+)1(\s*\r?\n\);\s*\r?\n\s*INSERT INTO user_profiles)", '$1TRUE$2'

$adminHash = '$2a$11$OyGY89bvQ8GiaKJkFxScce9RKyp/.D7RR0b7PoPm8Qy/FHU8YOTdS'
$adminReplacement = @"
-- =========================================================
-- 13. SAMPLE ADMIN ACCOUNT
-- Default admin login:
-- Email: admin@aiinterview.local
-- Password: Admin@123456
-- Password hash is generated with BCrypt.Net-Next.
-- =========================================================

WITH admin_user AS (
    INSERT INTO users (user_type, status)
    VALUES ('ADMIN', 'ACTIVE')
    RETURNING id
), admin_account AS (
    INSERT INTO authentication_accounts (
        user_id,
        email,
        password_hash,
        is_verified
    )
    SELECT
        id,
        'admin@aiinterview.local',
        '$adminHash',
        TRUE
    FROM admin_user
)
INSERT INTO user_profiles (
    user_id,
    full_name,
    education_level,
    career_goal
)
SELECT
    id,
    'System Administrator',
    NULL,
    'Manage AI Interview Platform'
FROM admin_user;
"@

$adminMarker = '-- =========================================================' + [Environment]::NewLine + '-- 13. SAMPLE ADMIN ACCOUNT'
$adminStart = $content.IndexOf($adminMarker)
if ($adminStart -lt 0) {
    $adminMarker = "-- =========================================================`n-- 13. SAMPLE ADMIN ACCOUNT"
    $adminStart = $content.IndexOf($adminMarker)
}
if ($adminStart -ge 0) {
    $content = $content.Substring(0, $adminStart) + $adminReplacement
}

$content = $content -replace '(?m)^;\s*$', ''

$header = @'
-- AI Interview Platform schema for Supabase/PostgreSQL
-- Generated from SQL Server script. Run this in Supabase SQL Editor or via a PostgreSQL client.

'@
Set-Content -LiteralPath $outPath -Value ($header + $content.Trim() + [Environment]::NewLine) -Encoding UTF8
Write-Output (Resolve-Path $outPath)
