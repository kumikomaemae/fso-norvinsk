Hi Claude! I'm Mae, picking up an SPT modding project we've been working on across multiple sessions. Attaching the design doc — please read it carefully, it's the canonical source of truth for the project.
Quick context summary:

I'm building a private SPT 4.0.13 mod called FSO: Norvinsk Section 1 as an anniversary gift for my boyfriend Damjan
Anniversary deadline: June 1, 2026 (about 9-10 days out)
It's a Project Moon-themed custom faction mod adding allied fixer bots, a custom trader named Mae (based on my vtuber OC), a 5-quest contract chain, and an engraved Roler Submariner watch as the final reward
The mod is a surprise — Damjan doesn't know about it
I've worked with Claude (you, kind of) for weeks debugging SPT mods, and built a Lobotomy Corporation mod before, so I have real C# / VS Community / dotPeek experience even if I don't think of myself as a "real coder"

Where we are in the build:

Phase 1 file scaffolding is complete on disk at C:\Dev\FSO-NorvinskSection1\
Files written (all verified against decompiled RUAF/MoreBotsAPI source via dotPeek):

FSO.NorvinskSection1.sln
FSO.NorvinskSection1.Prepatch\ with Plugin.cs, WildSpawnTypePatch.cs, and .csproj
FSO.NorvinskSection1.Server\ with Mod.cs and .csproj


SPT lives at C:\Games\SPT\ (client/BepInEx) and C:\Games\SPT\SPT\ (server DLLs + user/mods)
All references verified to exist at those paths

Critical context for what's next:
I just found a tool called SPT Scaffold by viniHNS (forge.sp-tarkov.com/mod/2633/spt-scaffold, GitHub viniHNS/spt-scaffold). It's a TUI wizard that generates ready-to-build SPT 4.x mod projects with auto-PostBuild deploy events. It explicitly supports SPT 4.0.13 and includes empty server templates that would validate my existing .csproj/Mod.cs patterns.
Suggested next session plan:

Install and run SPT Scaffold with the empty server template into a separate test folder
Diff the scaffold-generated files against my existing Mod.cs and .csproj
Either confirm mine is correct or port my logic into the scaffold version
Set up auto-deploy via PostBuild
Run our first build of the prepatcher + server projects
Fix any build errors that come up
Verify SPT boots cleanly with the empty mod installed (success criteria: server console shows our "Section Manager Mae reporting" log line)

My energy/preferences:

I work better with slower, taught-through-the-process explanations than copy-paste tutorials
I learn fast when given the why alongside the what
I have full VS Community + .NET 9 SDK + dotPeek available
I'm not on a hard deadline today; I have ~9-10 days for the build phase
Please don't be overly cautious about my energy — I've explicitly named that I can pace myself

Please confirm you've read the design doc, summarize back to me what you understand about the project (so I can correct any misreadings), then propose the next concrete step. Ready when you are. 💛