# Project Aura: Future Architecture Roadmap

This document outlines the planned technical upgrades and architectural decisions for Project Aura. Because the graph and ingestion systems will naturally grow in complexity, we are mapping the upcoming phases to keep the system performant, secure, and extensible.

## Phase 1: Real-Time Topology (SignalR Migration)
Currently, the Unity client operates on an aggressive HTTP polling loop to pull viewport data. This limits scalability as graph density increases.

### Proposed Changes
- **Aura.Api**: Introduce `Microsoft.AspNetCore.SignalR`. We will create an `AuraHub` that pushes spatial data deltas only when the `PhysicsEngineWorker` ticks or new entities are ingested.
- **Aura.Unity**: Replace the 1Hz `UnityWebRequest` loop in `AuraClient.cs` with the `.NET SignalR Client`. This establishes a persistent WebSocket connection. Unity will listen for `OnNodesUpdated` and `OnEdgesUpdated` events.
- **Impact**: Zero-latency visual updates, significant reduction in CPU/Network overhead, and immediate feedback during physics force calculations.

## Phase 2: Ingestion Ecosystem (The "Cyber" Footprint)
To seamlessly integrate the user's digital footprint (Likes, Comments, Saves on Youtube/Instagram) without navigating Meta/Google's enterprise API restrictions, we will build a local-first browser extension.

### Proposed Changes
- **Aura Desktop Extension**: A lightweight Chrome/Edge web extension built with vanilla JavaScript. 
- **Content Scripts**: Scripts injected onto specific domains (e.g., youtube.com, instagram.com). They listen for DOM click events on "Like" or "Save" buttons.
- **Background Worker**: When a monitored event fires, the background script quietly scrapes the element's context (Video Title, Description Snippet, URL) and makes an HTTP `POST` to the `Aura.Api`.
- **System Impact**: Data streams directly into Project Aura entirely locally. No third-party servers, Zapier subscriptions, or OAuth keys required.

## Phase 3: Spatial UX & Rendering
As the graph moves beyond simple placeholder spheres, the visualization layer will be enhanced to convey deeper meaning.

### Proposed Changes
- **Force-Directed Rendering**: Shift the true force-directed physics payload (repulsion and spring logic) completely into `PhysicsEngineWorker`. Unity will solely interpolate the resulting vectors.
- **Micro-Interaction System**: Implement a Unity Raycaster on the camera. Looking directly at an orbiting node expands its radius and reveals its raw textual data. Looking away smoothly collapses it back into its essence color profile.

## Phase 4: Cloud Deployment & Containerization (Docker)
Once the Local Proof of Concept (POC) is ready to migrate to the internet (allowing remote mobile ingestion or sharing), the backend will be containerized.

### Proposed Changes
- **Dockerization**: Create a `Dockerfile` for `Aura.Api` utilizing the `.NET 7` lightweight Linux Alpine images.
- **Data Persistence**: Map the `SQLite` `.db` files to a secure external Docker Volume so graph data survives container reboots.
- **Infrastructure Orchestration**: Build a `docker-compose.yml` to bundle the API with an NGINX reverse proxy for SSL termination.
- **Hosting Strategy**: Deploy seamlessly to an affordable cloud host (like a $5 DigitalOcean Droplet, AWS EC2, or Azure Container Apps) without worrying about Windows licensing or matching local SDKs.
