# FinReg Audit Platform — Project Context

## What This Project Does

FinReg Audit Platform is a production-quality financial compliance system that models how regulated firms maintain immutable audit trails of account and transaction activity under FCA (Financial Conduct Authority) oversight. The system implements Event Sourcing and CQRS with Clean Architecture to demonstrate the patterns used in production at firms like Monzo, Revolut, and Standard Life.

Every account state change in the system — an account opening, a transaction being initiated, a transaction being flagged for suspicious activity — is recorded as an immutable event. Nothing is ever updated or deleted. The complete history of any account can be reproduced by replaying its events from the beginning, exactly as it would appear in a regulatory audit.

The system surfaces suspicious activity automatically using simplified but credible FCA thresholds: single transactions above £10,000, cumulative transactions exceeding £25,000 in 24 hours, repeated failed transaction attempts indicating fraud, and dormant accounts with sudden large activity. These are flagged as alerts that compliance officers can review, acknowledge, and act upon.

## Why It Exists

This project is a portfolio piece targeting senior technical roles at UK fintech companies, European financial services firms, and compliance technology providers. The technical choices are intentional and defensible: the same architectural patterns appear in regulated financial services production systems because immutability, auditability, and traceability are not optional in this domain — they are regulatory requirements.

The goal is to demonstrate that the engineer who built this understands not just the mechanics of Event Sourcing and CQRS, but the business and regulatory reasons those patterns exist in financial services.

## Target Audience

**Primary:** Technical recruiters and hiring engineers at UK fintech companies (Monzo, Revolut, Starling, Wise, ClearBank, Form3), compliance technology companies (Encompass, ComplyAdvantage, Napier), and European financial services firms operating under FCA, MiFID II, DORA, or EBA frameworks.

**Secondary:** Engineering managers evaluating architectural maturity. The project is designed so that a technical hiring manager opening the Swagger UI can understand the domain and the architecture within two minutes.

## FCA Compliance Context

The Financial Conduct Authority (FCA) requires regulated firms to maintain complete, tamper-evident records of all financial activity. Under the Senior Managers and Certification Regime (SM&CR) and anti-money laundering regulations, firms must be able to produce a full audit trail for any account or transaction on request — sometimes years after the fact.

This system models four simplified but realistic regulatory triggers that would appear in a production AML (Anti-Money Laundering) system:

1. **Single transaction ≥ £10,000** — flagged for Suspicious Activity Report (SAR) consideration. This is close to the threshold at which firms must conduct enhanced due diligence under the Money Laundering Regulations 2017.

2. **Cumulative transactions > £25,000 in 24 hours** — flagged as a potential structuring pattern. Structuring (deliberately breaking up transactions to avoid reporting thresholds) is a criminal offence under the Proceeds of Crime Act 2002.

3. **5+ failed transaction attempts within 1 hour** — flagged as a potential fraud pattern. Repeated failed attempts can indicate account takeover or card testing.

4. **Dormant account (12+ months inactive) with a sudden large transaction** — flagged as high risk. Dormant account reactivation combined with large outflows is a common money mule pattern.

These thresholds are simplified for demonstration purposes. In production, AML systems use far more sophisticated models incorporating transaction networks, counterparty risk, and machine-learning-based anomaly detection.

## Event Sourcing Rationale — Why Not Just Update Records?

The obvious alternative to Event Sourcing is a traditional relational model: one row per account in a database, updated in place as state changes. This is simpler to build and reason about for most applications. So why use Event Sourcing here?

In regulated financial services, the answer is that immutability is a requirement, not a design choice. A regulator asking "what was the balance of account X at 14:32 on 15 March 2024, and what transactions had occurred by that point?" cannot be answered by a system that overwrites state. With Event Sourcing, the question is trivially answerable: replay all events for account X up to that timestamp. The current state is always a derivative of the event history, never the source of truth in itself.

Beyond compliance, Event Sourcing provides a complete audit trail as a first-class architectural feature rather than an afterthought. There is no separate audit log table that may or may not be kept in sync with the main state — the event log is the state. This eliminates an entire class of bugs where the audit trail diverges from reality.

The trade-off is complexity: Event Sourcing requires more careful upfront design, adds read complexity (rebuilding state from events is more work than reading a row), and requires snapshot strategies for performance at scale. These trade-offs are appropriate for compliance systems where auditability and correctness outweigh operational simplicity.

## Applicability to European Regulatory Frameworks

While the FCA thresholds and specific regulations referenced are UK-specific, the architectural patterns apply equally to European financial services operating under:

- **MiFID II** (Markets in Financial Instruments Directive) — requires complete transaction records for equity and derivatives trading, including best execution evidence.
- **DORA** (Digital Operational Resilience Act) — requires detailed logging of ICT-related incidents and operational events for financial entities.
- **EBA Guidelines on Internal Governance** — requires audit trails for material decisions and control activities across credit institutions.
- **GDPR Article 30** — requires records of processing activities; Event Sourcing's append-only log is a natural fit for demonstrating data processing history without modifying records.

## Portfolio Success Metrics — What a Recruiter Must See Within 2 Minutes

A technical recruiter opening the live Swagger UI must be able to:

1. See clearly labelled endpoint groups: Auth, Accounts, Transactions, Alerts, Reports.
2. Register or log in with demo credentials to obtain a JWT.
3. Authorise with the JWT using the "Authorize" button.
4. Open an account and see the resulting `AccountOpenedEvent` in the event history.
5. Initiate a transaction above the £10,000 threshold and see the `TransactionFlaggedEvent` and `SuspiciousActivityDetectedEvent` automatically generated.
6. Call the audit trail endpoint and receive a paginated, chronological list of immutable events.
7. Call the compliance report endpoint and receive aggregated statistics.

If a recruiter can complete this sequence without reading any documentation, the project has succeeded in communicating the architecture through the API itself.
