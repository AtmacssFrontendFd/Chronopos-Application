# Scratch Card Code Architecture

## Overview
The ChronoPOS scratch card system uses **two different codes** for different purposes:

---

## 1. Display Code (User-Facing)
**Format**: `ABCD-1234-EFGH-5678` (16 characters with dashes)

### Purpose
- **Primary user-facing identifier** for manual entry
- Designed for human readability and error detection
- What salespeople share with clients when QR scanning fails
- Used as fallback entry method in POS system

### Characteristics
- Base36 encoding (0-9, A-Z)
- Built-in checksum for validation
- Encodes creation date for verification
- Format validation without database lookup
- **This is what appears on printed scratch cards**

### Usage Flow
1. Salesperson gives client a scratch card
2. Client sees: **Display Code: QWER-1234-ASDF-5678**
3. If QR scan fails → Client enters display code manually
4. POS validates format + checksum
5. POS looks up card in database using display code
6. POS retrieves encrypted payload

---

## 2. Card Code (Internal Identifier)
**Format**: `41FOUMQQH2VT` (12 alphanumeric characters)

### Purpose
- **Internal database unique identifier**
- Used in cryptographic operations
- Part of the encrypted QR code payload
- Backend validation and card uniqueness

### Characteristics
- Cryptographically secure random generation
- Guaranteed unique in database
- Used for encryption/decryption validation
- **NOT intended for manual human entry**

### Usage Flow
1. Generated during scratch card creation
2. Embedded in encrypted QR code payload
3. Used internally for card validation
4. Linked to database records
5. Never manually entered by users

---

## QR Code Contents
The QR code contains the **full encrypted ScratchCardInfo**, which includes:

```json
{
  "CardCode": "41FOUMQQH2VT",           // Internal ID
  "DisplayCode": "QWER-1234-ASDF-5678",  // User-friendly ID
  "PlanId": 2,
  "PlanName": "Professional Plan",
  "PlanPrice": 59.99,
  "DurationInDays": 30,
  "SalespersonId": 5,
  "SalespersonName": "Jane Smith",
  "SalespersonEmail": "jane@chronopos.com",
  "CreatedAt": "2025-10-01T12:00:00Z",
  "ExpiryDate": "2026-10-01T12:00:00Z",
  "BatchNumber": "BATCH_20251001"
}
```

---

## POS System Entry Methods

### Method 1: QR Code Scan (Recommended)
1. User scans QR code with camera
2. POS decrypts entire payload
3. Gets all information instantly
4. No database lookup needed
5. **Fastest and most reliable**

### Method 2: Manual Display Code Entry (Fallback)
1. User enters: `QWER-1234-ASDF-5678`
2. POS validates format and checksum locally
3. POS searches database by DisplayCode
4. POS retrieves encrypted data from database
5. POS decrypts and validates
6. **Requires database connection**

### Method 3: Internal Card Code (NOT RECOMMENDED)
- Should NOT be exposed for manual entry
- Too long and error-prone
- Reserved for backend operations only

---

## UI Display Recommendations

### Admin Panel
**Scratch Card List View:**
- Primary: Display Code (large, prominent)
- Secondary: Card Code (small, technical details section)

**Scratch Card Details:**
- **TOP**: Display Code in large font with copy button
- Card Code: Hidden in "Advanced/Technical Details" section
- QR Code: Generated from encrypted data

### Printed Scratch Cards
```
┌─────────────────────────────────┐
│     ChronoPOS Activation        │
│                                 │
│  [QR CODE]                      │
│                                 │
│  Scan QR or enter code below:   │
│                                 │
│  ┌─────────────────────────┐   │
│  │  QWER-1234-ASDF-5678    │   │
│  └─────────────────────────┘   │
│                                 │
│  Plan: Professional Plan        │
│  Valid Until: Oct 2026          │
└─────────────────────────────────┘
```

### POS Onboarding Screen
```
┌──────────────────────────────────────┐
│  Enter Scratch Card Activation       │
│                                      │
│  [Scan QR Code]  or  [Enter Code]   │
│                                      │
│  Activation Code:                    │
│  ┌────┬────┬────┬────┐             │
│  │QWER│1234│ASDF│5678│             │
│  └────┴────┴────┴────┘             │
│                                      │
│  ✓ Valid format                     │
│  → Retrieving card details...       │
└──────────────────────────────────────┘
```

---

## Database Schema
```sql
CREATE TABLE ScratchCards (
    Id INTEGER PRIMARY KEY,
    CardCode TEXT UNIQUE NOT NULL,        -- Internal: 41FOUMQQH2VT
    DisplayCode TEXT UNIQUE NOT NULL,     -- User-facing: QWER-1234-ASDF-5678
    PlanId INTEGER NOT NULL,
    SalespersonId INTEGER NOT NULL,
    IsUsed BOOLEAN DEFAULT 0,
    CreatedAt DATETIME NOT NULL,
    UsedAt DATETIME,
    ExpiryDate DATETIME NOT NULL,
    EncryptedData TEXT NOT NULL,          -- Full encrypted payload
    BatchNumber TEXT
);

-- Indexes for fast lookup
CREATE INDEX idx_scratch_cards_display_code ON ScratchCards(DisplayCode);
CREATE INDEX idx_scratch_cards_card_code ON ScratchCards(CardCode);
```

---

## Security Considerations

1. **Display Code** can be validated offline (checksum + format)
2. **Card Code** requires decryption to validate
3. QR code contains full encrypted payload (no DB needed for validation)
4. Both codes must match when retrieved from database
5. Expiry date checked before activation

---

## Migration Path

### Current State
- UI shows CardCode prominently ❌
- DisplayCode exists but not highlighted ❌

### Desired State
- UI shows DisplayCode prominently ✅
- CardCode hidden or in technical section ✅
- QR code generation includes both ✅
- POS system searches by DisplayCode ✅

---

## Implementation Checklist

- [ ] Update ScratchCard list view to show DisplayCode
- [ ] Update ScratchCard details modal to prominently show DisplayCode
- [ ] Add "Copy Display Code" button
- [ ] Move CardCode to "Advanced Details" collapsible section
- [ ] Update print templates to show DisplayCode
- [ ] Add DisplayCode search filter
- [ ] Update POS developer documentation
- [ ] Create sample scratch card design with DisplayCode
