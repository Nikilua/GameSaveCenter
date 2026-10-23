Write-Output 'Failed ExampleTests.ControlledFailure'
Write-Output 'Assert.Equal() Failure: Expected: 1 Actual: 2'
[Console]::Error.WriteLine('Stack Trace: at ExampleTests.ControlledFailure():line 42')
Write-Output $env:GSC_DIAGNOSTIC_TEST_PATH
Write-Output 'https://alice:sample-secret@example.invalid/test?token=sample-token&ok=true'
exit 7