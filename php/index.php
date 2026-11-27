<?php
// index.php - the PHP part. A tiny save-game service that stores careers as JSON files.
// Run it with:  php -S 127.0.0.1:8082 -t php

header('Content-Type: application/json; charset=utf-8');

$dir = __DIR__ . '/saves';
if (!is_dir($dir)) {
    mkdir($dir, 0777, true);
}

function fail(int $status, string $message): void {
    http_response_code($status);
    echo json_encode(['error' => $message]);
    exit;
}

function slot_file(string $dir, string $slot): string {
    if (!preg_match('/^[A-Za-z0-9_-]{1,40}$/', $slot)) {
        fail(400, 'Invalid save slot.');
    }
    return $dir . '/' . $slot . '.json';
}

$action = $_GET['action'] ?? '';
$method = $_SERVER['REQUEST_METHOD'];

if ($action === 'health') {
    echo json_encode(['ok' => true]);

} elseif ($action === 'list' && $method === 'GET') {
    $saves = [];
    foreach (glob($dir . '/*.json') ?: [] as $file) {
        $wrapper = json_decode((string) file_get_contents($file), true);
        $player = $wrapper['data']['player'] ?? null;
        if (!is_array($player)) {
            continue;
        }
        $saves[] = [
            'slot'     => basename($file, '.json'),
            'name'     => (string) ($player['name'] ?? 'Unknown'),
            'club'     => (string) ($player['clubName'] ?? ''),
            'position' => (string) ($player['position'] ?? ''),
            'age'      => (int) ($player['age'] ?? 0),
            'savedAt'  => (string) ($wrapper['savedAt'] ?? ''),
        ];
    }
    usort($saves, fn($a, $b) => strcmp($b['savedAt'], $a['savedAt']));
    echo json_encode(['saves' => $saves]);

} elseif ($action === 'save' && $method === 'POST') {
    $file = slot_file($dir, $_GET['slot'] ?? '');
    $body = file_get_contents('php://input');
    if (strlen($body) > 300000) {
        fail(413, 'Save is too large.');
    }
    $data = json_decode($body, true);
    if (!is_array($data) || !isset($data['player']['name'])) {
        fail(400, 'Not a valid career.');
    }
    $wrapper = ['savedAt' => gmdate('c'), 'data' => $data];
    if (file_put_contents($file, json_encode($wrapper), LOCK_EX) === false) {
        fail(500, 'Could not write the save file.');
    }
    echo json_encode(['ok' => true]);

} elseif ($action === 'load' && $method === 'GET') {
    $file = slot_file($dir, $_GET['slot'] ?? '');
    if (!is_file($file)) {
        fail(404, 'Save not found.');
    }
    echo file_get_contents($file);

} else {
    fail(404, 'Unknown action.');
}
