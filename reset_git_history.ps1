# Reset Git history while keeping ignored files
# Assumes:
# - Repo is private
# - Only you use it
# - main is the primary branch
# - origin remote exists

Write-Host "Resetting Git history..." -ForegroundColor Cyan

# Safety check: ensure we're inside a git repo
if (-not (Test-Path ".git")) {
    Write-Error "This directory is not a Git repository."
    exit 1
}

# Show current branch
$currentBranch = git branch --show-current
Write-Host "Current branch: $currentBranch"

# Optional confirmation
$confirm = Read-Host "This will permanently delete all commit history. Continue? (y/n)"
if ($confirm -ne "y") {
    Write-Host "Operation cancelled."
    exit 0
}

# Create orphan branch
git checkout --orphan main-new

# Stage tracked files
git add -A

# Create new initial commit
git commit -m "Initial commit"

# Delete old main branch (if it exists)
git branch -D main 2>$null

# Rename new branch to main
git branch -m main

# Force push to origin
git push -f origin main

Write-Host "Git history successfully reset." -ForegroundColor Green



# HARD RESET GIT HISTORY (NO PROMPTS)

git checkout --orphan main-new
git add -A
git commit -m "Initial commit"
git branch -D main
git branch -m main
git push -f origin main
