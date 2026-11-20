---
name: schedule-deferred-tasks
description: Schedule a list of tasks to start at a given time and work through them on a 15-minute loop, in parallel where the tasks allow it, running each task on a subagent whose model and effort level match how complex the task is. Use when the user says "schedule deferred tasks", "run these tasks at 10pm", "work through the backlog tonight", "do these later", or names a start time for a batch of tasks.
---

# Schedule deferred tasks

Starts a loop at a time the user gives. Every 15 minutes the loop collects the tasks that have finished and hands the waiting tasks to subagents. Tasks run side by side whenever they do not get in each other's way, so the list is finished sooner, and the loop stops itself when the list is finished.

## Inputs

- **Start time.** Required. If the user gave none, ask once. "Now" is allowed: skip step 4 and go straight to "Starting the loop".
- **Tasks.** The list the user gives with the skill. If they gave no list, ask for one.

## Setting up

1. **Write the queue file** at `<repo root>/.claude/deferred-tasks/<YYYY-MM-DD-HHmm>.md`, named after the start time. The repo root is the top of the git repository the session is working in, or the working directory when there is none. The loop reads this file on every tick, so it must stand alone without the conversation. One entry per task, numbered in the order the user listed them:

   ```markdown
   ## 1. <short title>
   - status: pending        (pending | running | done | failed | blocked)
   - directory: <absolute path the task works in>
   - model: <haiku | sonnet | opus | fable>
   - effort: <low | medium | high | xhigh>
   - waits for: <numbers of the tasks that must be done first, or none>
   - brief: <everything a fresh subagent needs: goal, files, what done looks like, what is out of scope>
   - result: <filled in when the task finishes>
   ```

2. **Work out which tasks can run side by side.** A task waits for another task only when it needs that task's output or edits the same files. Record this in `waits for`; leave it as `none` wherever possible, because every task that waits makes the list slower.

3. **Pick the model and effort for each task** with the table below, and record them in the queue file. When a task sits between two rows, pick the cheaper row; a failed task is retried one row up.

   | Task looks like | Model | Effort |
   |---|---|---|
   | Mechanical and fully specified: a lookup, a rename, a format fix, a status check | haiku | low |
   | A clear change in a few files, routine tests, documentation | sonnet | medium |
   | A feature across several files, a bug with an unknown cause, a refactor | opus | high |
   | Design-level or ambiguous work, subtle correctness, heavy research | fable | high, or xhigh when a wrong answer is costly |

   Never use `max` effort unless the user asks for it by name.

4. **Schedule the start** with `CronCreate`, `recurring: false`, with the minute, hour, day and month pinned to the start time. The prompt is:

   > Start the deferred task loop for queue file `<absolute path>`. Follow the "Starting the loop" section of the schedule-deferred-tasks skill.

5. **Tell the user** the start time that `CronCreate` reported, the number of tasks, the model and effort chosen for each, which tasks wait for others, and the two limits in "Limits" below.

## Starting the loop

Runs when the one-off job fires.

1. Create the recurring job with `CronCreate`, `recurring: true`, firing every 15 minutes on off-round minutes such as `7,22,37,52 * * * *`. The prompt is:

   > Deferred task loop tick for queue file `<absolute path>`. Follow the "Each tick" section of the schedule-deferred-tasks skill.

2. Write the job ID at the top of the queue file as `loop job: <id>`.
3. Run the first tick straight away.

## Each tick

1. Read the queue file.
2. **Collect.** For every task marked `running` whose subagent has reported back, write a one-line result and set its status to `done`, `failed` or `blocked`. A task that failed once goes back to `pending` one row up the table; a task that failed twice stays `failed`.
3. **Launch.** Start every `pending` task whose `waits for` tasks are all `done`, until five tasks are running. Launch them in one message so they run side by side, each with the `Agent` tool using the `model` and `effort` from the queue file, and mark each one `running`. A task that waits for a `failed` or `blocked` task is marked `blocked` too; the other tasks carry on.
4. **Finish.** When no task is `pending` or `running`, delete the loop job with `CronDelete` using the ID in the queue file, and give the user a summary: one line per task with its status and result, then anything that needs a decision from them.

A tick that finds nothing to collect and nothing to launch ends without a message. When a subagent reports back between ticks, record its result in the queue file then, and leave launching to the next tick.

## The subagent brief

Each brief contains the task's `brief` and `directory` from the queue file, plus these rules, word for word:

- Do only this task. Report anything else you notice instead of fixing it.
- Do not commit, push, send messages, publish, or delete anything outside the task. Leave the changes in the working tree and give back the proposed commit message for review.
- Docstrings, comments and commit text are full plain sentences that a reader outside the speciality can follow, kept to one or two sentences.
- Do not build, run or test Apple-only projects (Xcode, Swift) on this Windows machine.
- If the task needs a decision or access you do not have, stop and say exactly what is needed. That task is then marked `blocked`.
- Finish with: what changed (files), how it was checked, and what is still open.

## Limits

- The schedule lives only in the current Claude Code session. Closing the session cancels the start job and the loop; the queue file survives, and running the skill again on that file resumes the tasks still `pending`.
- Jobs fire only while the session is idle, and each tick can run a few minutes late. A recurring job expires on its own after 7 days.
