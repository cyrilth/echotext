# EchoText - Claude Code Development Guide

**Purpose:** Best practices for developing EchoText using Claude Code with Subagents.

---

## 1. Initial Setup

### 1.1 Project Folder Structure

Extract the downloaded zip to your Windows machine:

```
C:\Projects\echotext\
├── .claude\
│   └── agents\              ← Subagents (auto-loaded by Claude Code)
│       ├── verifier.md
│       ├── reviewer.md
│       ├── task-runner.md
│       ├── debugger.md
│       ├── test-writer.md
│       └── arch-checker.md
├── docs\
│   ├── REQUIREMENTS.md
│   ├── ARCHITECTURE.md
│   ├── TASKS.md
│   └── CLAUDE_CODE_GUIDE.md
├── CLAUDE.md                ← Auto-read by Claude Code
└── src\                     ← Claude will create this
```

### 1.2 Start Claude Code

```bash
cd C:\Projects\echotext
claude
```

---

## 2. Available Subagents

Subagents are specialized AI agents with their own context window. They're automatically loaded from `.claude/agents/`.

| Subagent | Purpose | When to Use |
|----------|---------|-------------|
| **task-runner** | Implement tasks | Starting a new task |
| **verifier** | Verify & commit | After implementation |
| **reviewer** | Code quality review | Before committing (optional) |
| **debugger** | Fix errors | Build/test failures |
| **test-writer** | Write unit tests | Need test coverage |
| **arch-checker** | Architecture compliance | Periodic checks |

### View All Agents
```
/agents
```

---

## 3. First Prompt

```
Read all files in /docs folder for project context, then start TASK-101 from TASKS.md.
```

Claude Code auto-reads CLAUDE.md, so it already knows the project basics.

---

## 4. Development Workflow

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                              WORKFLOW                                       │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│  SESSION START                                                              │
│  ────────────                                                               │
│  "Read /docs and show project status"                                       │
│                                                                             │
│  TASK LOOP (repeat)                                                         │
│  ─────────────────                                                          │
│  1. "Use task-runner for TASK-101"      ← Implement                         │
│  2. "Use reviewer for TASK-101"         ← Review (optional)                 │
│  3. "Use verifier for TASK-101"         ← Verify + Commit                   │
│  4. "What's next?"                      ← Next task                         │
│                                                                             │
│  SESSION END                                                                │
│  ───────────                                                                │
│  "Summarize progress"                                                       │
│  git push                                                                   │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

---

## 5. Subagent Prompts

### Task Implementation
```
Use task-runner for TASK-101
```

### Verify & Commit
```
Use verifier for TASK-101
```

### Code Review (Optional)
```
Use reviewer for TASK-101
```

### Fix Errors
```
Use debugger to fix the build errors
```

### Write Tests
```
Use test-writer for ConfigService
```

### Architecture Check
```
Use arch-checker to verify the codebase
```

---

## 6. Command Flow Summary

### Standard Flow (Recommended)
```
You: Use task-runner for TASK-101
     ↓
You: Use verifier for TASK-101
     ↓
You: What's next?
```

### With Code Review
```
You: Use task-runner for TASK-101
     ↓
You: Use reviewer for TASK-101
     ↓
You: Use verifier for TASK-101
     ↓
You: What's next?
```

### Fixing Issues
```
You: Use verifier for TASK-101
Claude: "❌ Build failed..."
     ↓
You: Use debugger to fix it
     ↓
You: Use verifier for TASK-101
```

---

## 7. Example Session

```
# Start Claude Code
> claude

# First prompt
You: Read /docs folder, then start TASK-101

Claude: [reads docs, summarizes understanding]
        Starting TASK-101...
        [creates solution structure]

# Verify and commit
You: Use verifier for TASK-101

Claude: [verifier agent runs]
        ✅ All acceptance criteria met
        ✅ Build passed
        ✅ Committed: "TASK-101: Create Solution Structure"
        ✅ Updated TASKS.md

# Next task
You: Use task-runner for TASK-102

Claude: [task-runner agent implements]
        Done. Ready for verification.

# Continue...
You: Use verifier for TASK-102

# End session
You: Summarize today's progress

Claude: Completed: TASK-101, TASK-102
        Build: Passing
        Next: TASK-103

You: git push
```

---

## 8. Quick Reference

| Action | Prompt |
|--------|--------|
| Start task | `Use task-runner for TASK-XXX` |
| Verify & commit | `Use verifier for TASK-XXX` |
| Code review | `Use reviewer for TASK-XXX` |
| Fix errors | `Use debugger to fix it` |
| Write tests | `Use test-writer for ServiceName` |
| Check architecture | `Use arch-checker` |
| Project status | `What's the project status?` |
| End session | `Summarize progress` |

---

## 9. Tips

1. **Let subagents work** - They have their own context, let them complete
2. **One task at a time** - Don't rush to the next task
3. **Always verify** - Use verifier before moving on
4. **Git push often** - After verifier commits, push to remote
5. **Check /agents** - See all available subagents

---

## 10. Troubleshooting

### Subagent Not Found
```
/agents
```
Check if agents are in `.claude/agents/` folder.

### Build Failing
```
Use debugger to fix the build errors
```

### Architecture Drift
```
Use arch-checker to verify compliance
```

### Need to Reset
```
Stop. Read docs/ARCHITECTURE.md section X and docs/TASKS.md TASK-XXX.
Start fresh on this task.
```

---

## Revision History

| Version | Date | Changes |
|---------|------|---------|
| 2.0 | 2025-01-28 | Rewritten for Subagents |
| 1.0 | 2025-01-28 | Initial guide |
