# Version 1.1

# Man kann jetzt verschiedene Gegnertypen in einem Level spawnen lassen! (LevelManager hat dafür ein Dictionary aus verschiedenen ObjectPools)

# Blobs - Enemy Pool & Splitting System

- Kinder werden auch aus dem EnemyPool geholt statt immer neu erzeugt zu werden
- Kinder haben weniger Leben / SprocketCost abhängig vom SplitLevel
- Bug behoben, dass Kinder immer erst wieder zum allerersten Waypoint zurückgelaufen sind


# Balancing Türme

## Canon (Langsam aber guter Schaden)

| Attribut | Wert |
|---------|------|
| Schaden | 15 |
| Feuerrate | 2 |
| Leben | 700 |
| Kosten | 20 |

## Tesla (Greift 3 Ziele an aber etwas weniger Schaden)

| Attribut | Wert |
|---------|------|
| Schaden | 5 |
| Feuerrate | 2 |
| Leben | 1100 |
| Kosten | 100 |

## Crossbow (Schnelle Feuerrate und hohe Reichweite aber wenig Schaden)

| Attribut | Wert |
|---------|------|
| Schaden | 2 |
| Feuerrate | 0.2 |
| Leben | 1300 |
| Kosten | 200 |

---

# Balancing Gegner

## HeliToad (hält 2 Kanonenschüsse aus)

| Attribut | Wert |
|---------|------|
| Leben | 30 |
| Kollisionsschaden | 1 |
| Sprockets | 30 |

## FloatToad (Mehr Leben, langsam, viel Schaden am Gate)

| Attribut | Wert |
|---------|------|
| Leben | 60 |
| Kollisionsschaden | 3 |
| Sprockets | 60 |

## IceBall (Verlangsamt Türme, schnell, wenig Leben)

| Attribut | Wert |
|---------|------|
| Verlangsamung | 1.4 |
| Leben | 15 |
| Kollisionsschaden | 1 |
| Sprockets | 30 |

## Blob (Teilt sich beim Tod in zwei kleinere KindBlobs auf)

| Attribut | Wert |
|---------|------|
| Maximales SplitLevel | 3 |
| Leben | 30 (halbiert sich pro SplitLevel) |
| Kollisionsschaden | 1 |
| Sprockets | 40 → danach 40/SplitLevel |
