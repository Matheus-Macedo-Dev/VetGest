# VetGest - Master Specification (MVP and Architecture)

## 1. Product overview

VetGest is a Progressive Web App adapted to veterinary pregnancy care. It gives the pet owner (Tutor) a simple way to track progress, receive reminders, and recognize safety alerts while sharing relevant records with a veterinarian.

VetGest supports organization and education; it does not replace veterinary consultation, diagnosis, or treatment. Estimated dates and educational content must be presented with appropriate uncertainty and veterinary review.

## 2. Architecture and infrastructure

- **Frontend PWA:** Blazor WebAssembly with responsive, mobile-first UI and carefully scoped offline support.
- **3D rendering:** Three.js or Babylon.js through Blazor JavaScript interop, loaded on demand.
- **Backend API:** ASP.NET Core Web API organized using Clean Architecture.
- **Database:** Azure SQL Database accessed through Entity Framework Core.
- **Vet-Tutor synchronization:** SignalR for real-time updates, with server-side authorization and data-isolation checks.
- **Authentication:** ASP.NET Core Identity and JWT bearer authentication with the roles `Vet` and `Tutor`.

## 3. Main user flow

1. The Tutor registers a pet and pregnancy information.
2. The system calculates gestational age and an estimated due date.
3. The app identifies the current phase and presents the corresponding fetal development content.
4. The Tutor receives basic care guidance and upcoming reminders.
5. The system displays scheduled evaluations and examinations.
6. The Tutor records weight, appetite, behavior, temperature, symptoms, photos, and observations in the diary.
7. The app evaluates configured alert rules and advises when veterinary contact may be needed.
8. A linked Vet can review and update authorized records in real time.

## 4. MVP scope: five pillars

The MVP prioritizes daily utility over an extensive academic library.

| Pillar | Content | Implementation |
| --- | --- | --- |
| Tracking | Gestational age, estimated due date, current phase, fetal development, and maternal changes | Today screen |
| Essential care | Diet, hydration, activity, environment, and handling | Short phase-based guidance |
| Exams and reminders | Pregnancy confirmation, ultrasound, radiography, and pre-birth consultations | Calendar and supported notifications |
| Alerts and birth | Normal signs, attention signs, emergency signs, preparation, and dystocia warnings | Traffic-light alert system |
| Gestational diary | Weight, appetite, behavior, temperature, symptoms, photos, and notes | Diary timeline and charts |

## 5. Registration data

### Pet

- Species: dog or cat
- Name
- Breed
- Age or date of birth
- Current weight
- Photo, if provided

### Pregnancy

- Mating date or dates
- Ovulation or progesterone date, when available
- Pregnancy confirmation status
- Estimated due date and calculation basis

### History

- Reproductive history
- Previous births
- Previous dystocia or cesarean sections
- Existing conditions
- Current medications
- Relevant veterinary notes

The interface must clearly state that the due date is an estimate. When the calculation is based on mating rather than ovulation, the uncertainty should be explained in plain language.

## 6. Core screens and features

### 6.1 Today screen: "Minha Gestação Hoje"

- Pet name, photo, species, and breed
- Gestational age, days remaining, and estimated due date
- Current phase with a summary of fetal development
- Optional 3D visualization with a non-WebGL fallback
- Expected maternal changes and phase-specific care guidance
- Next examination or reminder
- Quick access to the diary
- Active alert banner when a recent entry matches a configured alert rule

### 6.2 Phase system

The MVP uses broad phases instead of day-by-day content:

1. **Initial phase:** Fertilization and early embryo development.
2. **Intermediate phase:** Organogenesis and maternal changes.
3. **Growth phase:** Bone mineralization and increased maternal demand.
4. **Final phase:** Fetal maturation and signs that birth may be approaching.
5. **Birth and immediate postpartum:** Safety alerts and initial newborn-care guidance.

Phase durations and content must be curated separately for dogs and cats when the evidence differs.

### 6.3 Alert system

Alerts are educational prompts, not diagnoses.

| Level | Meaning | Example response |
| --- | --- | --- |
| Normal | Expected change that can be observed and logged | Continue monitoring and record relevant changes |
| Attention | A sign that warrants contacting the veterinarian | Contact the veterinary team for guidance |
| Emergency | A potentially serious sign requiring immediate veterinary contact | Seek urgent veterinary care |

Examples must be reviewed by a veterinarian. The app must not prescribe medication, provide invasive home procedures, or encourage delaying emergency care.

### 6.4 Vet-Tutor connection

- A Vet creates a short-lived invitation.
- The Tutor accepts the invitation through an authenticated flow.
- The API validates the invitation, prevents reuse, links the authorized records, and records the event.
- Both parties can only access records permitted by server-side resource authorization.
- SignalR broadcasts updates only to authorized connections.

## 7. Scientific content requirements

Scientific content should use plain language and separate dogs and cats whenever their needs or timelines differ. Content should be reviewed against current veterinary references, such as the Merck Veterinary Manual, before publication.

| Category | Content needed |
| --- | --- |
| Duration and phases | Typical ranges, calculation limits, and ovulation-versus-mating differences |
| Fetal development | Implantation, organogenesis, bone mineralization, and maturation |
| Maternal changes | Gastrointestinal, behavioral, weight, and mammary changes |
| Diet | Nutritional needs by phase and warnings against inappropriate supplementation |
| Examinations | Appropriate windows for ultrasound, radiography, and veterinary review |
| Birth | Environment preparation, intervals between births, and signs of labor |
| Alerts | Signs associated with dystocia, metritis, hypocalcemia, and eclampsia |

### Editorial rules

- The app guides and informs; it does not replace veterinary consultation.
- Label estimates, uncertainties, and species-specific differences clearly.
- Do not promise a diagnosis or a guaranteed outcome.
- Do not prescribe medication or teach invasive obstetric procedures.
- Every emergency message should direct the user to immediate veterinary care.

## 8. Out of scope for the MVP

- A complete encyclopedia of reproductive diseases
- Detailed pharmacological protocols or automatic prescriptions
- Day-by-day content for the entire pregnancy
- Home obstetric intervention tutorials
- Complex third-party integrations
- Highly advanced clinical dashboards

## 9. Developer checklist

- [ ] Set up Azure infrastructure: Azure SQL, App Service, and Static Web Apps.
- [ ] Implement authentication, authorization, and server-side data isolation.
- [ ] Create registration, login, and Vet-Tutor connection flows.
- [ ] Implement gestational calculation logic using mating and ovulation dates.
- [ ] Build the Today screen and its accessible visualization fallback.
- [ ] Develop the phase-based Care module.
- [ ] Create the examination and reminder calendar.
- [ ] Add supported web push notifications with permission and failure states.
- [ ] Implement the gestational diary and weight/temperature charts.
- [ ] Implement and test the traffic-light alert rules.
- [ ] Populate the five phases with veterinarian-reviewed content for dogs and cats.
- [ ] Add integration tests for authorization and linked-record isolation.

## 10. Value summary

The Tutor registers a pregnant pet, VetGest calculates an estimated phase, the app explains relevant development and care, reminders support veterinary follow-up, and the shared diary helps the Tutor and Vet monitor changes together.
