# Automated tests

Unity requires test code to live inside the Unity project, so the automated tests
are in [`src/HindsightGame/Assets/_Project/Tests`](../src/HindsightGame/Assets/_Project/Tests):

- `EditMode/` - NUnit unit tests for the core game rules (procedure flow,
  hold-still/tap/quiz rules, unlocks, progress saving), plus content validation of
  the authored procedure data.
- `PlayMode/` - end-to-end tests that boot the game and play the Eye Drops level
  through its real UI, including the retry paths (wrong quiz answers, blinking
  during the eye drop).

See [`src/HindsightGame/README.md`](../src/HindsightGame/README.md#tests) for how
to run them.
