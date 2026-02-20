# BillValidatorKeepAlive

//# Bill Validator Keep-Alive Simulation

//This project simulates a **bill validator keep-alive mechanism** in C#.  
//The system sends a keep-alive ping every 10 seconds.  
//The bill validator responds with an ACK (or no response, based on CLI input).  

//If no response is received within a timeout window (2 seconds):
//-An error alert is raised.
//- The system transitions to **MAINTENANCE** state.
//- Idempotency is ensured (no repeated spam transitions).

//---

//## Features
//- Keep-alive ping every 10 seconds.
//- ACK simulation toggle via CLI.
//- Error handling with timeout detection.
//- State machine with `RUNNING` → `MAINTENANCE` transition.
//- Idempotent state transitions.

//---

//## CLI Commands
//You can toggle ACK responses using:

//```bash
//device bill_validator ack on
//device bill_validator ack off
