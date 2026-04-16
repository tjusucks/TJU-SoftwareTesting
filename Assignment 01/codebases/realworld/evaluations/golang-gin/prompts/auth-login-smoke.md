/blackbox-testing:blackbox-testing  
Use this feature spec to generate black-box tests.

Feature file:  
Assignment 01/codebases/realworld/specification/features/auth-login-smoke.md

Target:  
Assignment 01/codebases/realworld/implementations/golang-gin

Output requirements:

- summarize the feature briefly
- generate black-box test scenarios
- then generate concrete Go API test cases
- focus only on externally visible behavior
- do not use internal source code or existing repo tests
- generate tests that can be run independently of the implementation

Output structure:

Assignment 01/codebases/realworld/evaluations/golang-gin/generated/auth-login-smoke.md: Description of the generated tests.
Assignment 01/codebases/realworld/evaluations/golang-gin/scripts/auth-login-smoke.sh: Shell script to run the generated tests.
Assignment 01/codebases/realworld/evaluations/golang-gin/tests: Go test files.
