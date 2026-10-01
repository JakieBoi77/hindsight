# Hindsight: Ophthalmology Preparation Game

## Project Overview

Children can experience anxiety during ophthalmology appointments. This anxiety
may reduce their cooperation and affect the quality of their assessment. Hindsight
is a proposed interactive mobile application that helps children prepare for
ophthalmology visits through age-appropriate games and activities.

The app introduces children and their families to common procedures, including
vision testing, eye drops, and slit-lamp examinations. Children can practise at
home, earn rewards, and become more familiar with what to expect before an
appointment. The project follows a user-centred design approach involving
clinicians, child-life specialists, parents, and children.

The project will pilot and evaluate the app's usability, feasibility, potential
to reduce anxiety, effect on cooperation, and impact on clinic efficiency.

## Team

- Ahsan Muzammil
- Mohammad Bilal
- Jackson Beach
- Ethan McMehen
- Jake Finlay

Project start date: September 21, 2026

## Repository Status

This repository currently contains the project's requirements, design,
planning, verification and validation, user documentation, research material,
presentation material, and generated PDF artifacts. `src/HindsightGame/` contains
the Unity proof-of-concept: a title screen, level select, and a complete Eye Drops
walkthrough with automated EditMode and PlayMode tests. See
[`src/HindsightGame/README.md`](src/HindsightGame/README.md) to open, run, test,
and build it.

## Repository Structure

- `docs/` - Project documentation and course deliverables:
	- `CDs/` - Custom-document expectations, including usability testing, user
		manuals, wireframes, machine learning, interviews, and related activities.
	- `Checklists/` - Checklists for requirements, design, code, verification,
		validation, and project documents.
	- `Design/` - Software architecture and detailed design documents.
	- `DevelopmentPlan/` - Development planning documentation.
	- `HazardAnalysis/` - Hazard analysis documentation.
	- `Presentations/` - Proof-of-concept, demonstration, final presentation,
		and EXPO materials.
	- `ProblemStatementAndGoals/` - Problem statement and project goals.
	- `projMngmnt/` - Productivity and project-management reports.
	- `ReflectAndTrace/` - Reflection and traceability documentation.
	- `SRS/` and `SRS-Volere/` - Software Requirements Specification documents.
	- `UserGuide/` - User guide documentation.
	- `VnVPlan/` and `VnVReport/` - Verification and validation plan and report.
	- Shared text files such as `Common.text`, `Comments.text`, and reflection
		files used by the LaTeX documents.
- `pdfs/` - Generated PDF versions of project documents, organized to match
	relevant sections of `docs/`.
- `refs/` - Reference material and the BibTeX bibliography in
	`References.bib`.
- `src/` - Application source code. `src/HindsightGame/` is the Unity 6 project.
- `test/` - Pointer to the automated tests, which live inside the Unity project
	(`src/HindsightGame/Assets/_Project/Tests`).
- `site/` - Static GitHub Pages template and stylesheet for browsing generated
	PDFs.
- `.github/` - Issue and pull-request templates plus the workflow and scripts
	used to build and publish documentation.
- `Makefile` - Local build rules for compiling LaTeX and Markdown documents to
	PDF, along with a `clean` target.
- `INSTALL.md` - Installation and setup notes.
- `CONTRIBUTING.md`, `CodeOfConduct.md`, and `LICENSE` - Project contribution,
	conduct, and licensing information.


The GitHub Actions workflow in `.github/workflows/latex-pages.yml` builds the
documentation and publishes the generated PDFs through GitHub Pages.

The documentation for this project is updated on the project's [GitHub page](https://team-hindsight.github.io/ophthamology-game/).