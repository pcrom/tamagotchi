<?php
header('Content-Type: application/json');

require 'dbAuth.php';

// Configuration
$hunger_decay_rate = 5; // Hunger points gained per hour away

try {
    $pdo = new PDO("mysql:host=$host;dbname=$db", $user, $pass);
    $pdo->setAttribute(PDO::ATTR_ERRMODE, PDO::ERRMODE_EXCEPTION);
    
    if ($_SERVER['REQUEST_METHOD'] === 'POST') {
        // Collect Security and Identification Data
        $userKey = $_POST['user_key'] ?? ''; // Account-level key
        $petKey  = $_POST['pet_key'] ?? '';  // Pet-specific key

        // Validate Authorization
        $stmt = $pdo->prepare("SELECT * FROM pets WHERE user_key = ? AND pet_key = ? LIMIT 1");
        $stmt->execute([$userKey, $petKey]);
        $currentPet = $stmt->fetch(PDO::FETCH_ASSOC);

        if (!$currentPet) {
            http_response_code(401);
            echo json_encode(["status" => "error", "message" => "Unauthorized access: Key mismatch."]);
            exit;
        }

        // Catch-up Logic (Hunger Decay)
        $lastUpdate = strtotime($currentPet['last_updated']);
        $hoursOffline = (time() - $lastUpdate) / 3600;
        
        // Incoming stats from C#
        $inHunger = isset($_POST['hunger']) ? (int)$_POST['hunger'] : (int)$currentPet['hunger'];
        $inExp    = isset($_POST['experience']) ? (int)$_POST['experience'] : (int)$currentPet['experience'];

        // Apply offline hunger gain
        $finalHunger = min(100, $inHunger + ($hoursOffline * $hunger_decay_rate));

        // Prepare Full Update (Including Visual Indices)
        // This ensures every stat, from experience to hats, is saved.
        $updateSql = "UPDATE pets SET 
            hunger = ?, experience = ?, 
            hair_index = ?, hat_index = ?, hold_index = ?, face_index = ?, 
            horn_index = ?, tail_index = ?, ears_index = ?, eyes_index = ?, 
            nose_index = ?, mouth_index = ?, 
            last_updated = NOW() 
            WHERE pet_key = ?";
        
        $updateStmt = $pdo->prepare($updateSql);
        $updateStmt->execute([
            $finalHunger,
            $inExp,
            $_POST['hair_index'] ?? $currentPet['hair_index'],
            $_POST['hat_index'] ?? $currentPet['hat_index'],
            $_POST['hold_index'] ?? $currentPet['hold_index'],
            $_POST['face_index'] ?? $currentPet['face_index'],
            $_POST['horn_index'] ?? $currentPet['horn_index'],
            $_POST['tail_index'] ?? $currentPet['tail_index'],
            $_POST['ears_index'] ?? $currentPet['ears_index'],
            $_POST['eyes_index'] ?? $currentPet['eyes_index'],
            $_POST['nose_index'] ?? $currentPet['nose_index'],
            $_POST['mouth_index'] ?? $currentPet['mouth_index'],
            $petKey
        ]);

        echo json_encode([
            "status" => "success",
            "new_hunger" => (int)$finalHunger,
            "offline_hours" => round($hoursOffline, 2)
        ]);

    } else {
        echo json_encode(["status" => "error", "message" => "POST requests only."]);
    }

} catch (PDOException $e) {
    echo json_encode(["status" => "error", "message" => "DB Error: " . $e->getMessage()]);
}
?>
