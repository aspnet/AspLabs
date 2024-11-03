# CSP

## Description

This directory contains .NET Core middleware for Content Security Policy (CSP). CSP is a very popular security mitigation against XSS and other injection vulnerabilities. CSP comes in many flavours, but we've chosen to add support for the most robust of them: nonce-based, strict-dynamic CSP.

Design document: [Implementing CSP Support in .NET Core](https://docs.google.com/document/d/13NPKn65Wf1PdIwNL7H0cxhwmp2r8ZTe6vizXzO2HqY4/edit#)
There was a previous discussion about CSP in .NET [here](https://github.com/dotnet/aspnetcore/issues/6001), that we have considered for our design.

## Contributions
This directory includes the following changes:

* Allow configuration of whether CSP enabled in reporting or enforcement modes.
* Allows configuration of a report URI, for violation reports sent by the browser.
* CSP middleware generates a nonce-based, strict-dynamic policy.
* Middleware adds the policy to HTTP responses according to the configuration.
* Custom <script> TagHelper to set nonce attribute on script blocks automatically.
* Provides a default implementation of a CSP violation report collection endpoint.
* Example app that uses our CSP middleware and corresponding basic unit tests.

## Usage:

```
// CSP configuration. Must come first because other middleware might skip any following middleware.

	app.UseCsp(policyBuilder =>
policyBuilder.WithCspMode(CspMode.ENFORCING)
	
.WithReportingUri("/csp"));
```
You can find the sample app under `./test/testassets/CspApplication/` directory.

# Point of contact
* Barry Dorrans - barry.dorrans@microsoft.com

## Authors
* Co-authored-by: Aaron Shim - aaronshim@google.com
* Co-authored-by: Santiago Diaz - salchoman@gmail.com