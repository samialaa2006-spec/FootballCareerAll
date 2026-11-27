// NewsServer.java - the Java part. A tiny news service that writes headlines about the player.
// It uses only the JDK (no libraries). Run it with:  java java/NewsServer.java
import com.sun.net.httpserver.HttpExchange;
import com.sun.net.httpserver.HttpServer;

import java.io.IOException;
import java.io.OutputStream;
import java.net.InetAddress;
import java.net.InetSocketAddress;
import java.net.URLDecoder;
import java.nio.charset.StandardCharsets;
import java.util.ArrayList;
import java.util.Collections;
import java.util.HashMap;
import java.util.List;
import java.util.Locale;
import java.util.Map;
import java.util.Random;

public class NewsServer {

    public static void main(String[] args) throws IOException {
        int port = args.length > 0 ? Integer.parseInt(args[0]) : 8081;

        HttpServer server = HttpServer.create(new InetSocketAddress(InetAddress.getLoopbackAddress(), port), 0);
        server.createContext("/health", ex -> send(ex, 200, "{"ok":true}"));
        server.createContext("/news", NewsServer::news);
        server.start();
        System.out.println("Java news service running on http://127.0.0.1:" + port);
    }

    private static void news(HttpExchange ex) throws IOException {
        Map<String, String> q = parseQuery(ex.getRequestURI().getRawQuery());

        String name = limit(q.getOrDefault("name", "The player"), 24);
        String position = limit(q.getOrDefault("position", ""), 4);
        String club = limit(q.getOrDefault("club", "the club"), 32);
        String nat = limit(q.getOrDefault("nat", ""), 24);
        int age = num(q.get("age"), 17);
        int ovr = num(q.get("ovr"), 50);
        int goals = num(q.get("goals"), 0);
        int assists = num(q.get("assists"), 0);
        long value = num(q.get("value"), 0);
        int season = num(q.get("season"), 0);

        List<String> out = headlines(name, position, club, nat, age, ovr, goals, assists, value, season);

        StringBuilder json = new StringBuilder("{"headlines":[");
        for (int i = 0; i < out.size(); i++) {
            if (i > 0) json.append(',');
            json.append('"').append(escape(out.get(i))).append('"');
        }
        json.append("]}");
        send(ex, 200, json.toString());
    }

    static List<String> headlines(String name, String position, String club, String nat, int age, int ovr, int goals, int assists, long value, int season) {
        Random rnd = new Random((name + "|" + club + "|" + season + "|" + ovr).hashCode());
        List<String> pool = new ArrayList<>();
        String money = String.format(Locale.ROOT, "€%.1fM", value / 1_000_000.0);

        if (season == 0) {
            pool.add(name + " signs a youth contract with " + club + " and joins the first-team squad");
            pool.add("Scouts call " + name + " one to watch at " + age);
            if (!nat.isEmpty()) pool.add(nat + " youth coaches are keeping a close eye on " + name + " at " + club);
        }
        if (goals >= 25) pool.add(name + " fires " + club + " forward with " + goals + " career goals");
        else if (goals >= 10) pool.add(name + " reaches " + goals + " goals in " + club + " colours");
        if (assists >= 12) pool.add(name + " is " + club + "'s chief creator with " + assists + " assists");
        if (value >= 80_000_000L) pool.add("Record bid rumoured for " + club + " star " + name + ", now valued at " + money);
        else if (value >= 30_000_000L) pool.add("European giants are watching " + club + "'s " + name + " (" + money + ")");
        else if (value >= 8_000_000L && age <= 22) pool.add(name + " is one of the brightest young talents at " + club + " (" + money + ")");
        if (age <= 20 && ovr >= 68) pool.add("Wonderkid alert: " + club + " teenager " + name + " hits " + ovr + " OVR");
        if (!nat.isEmpty() && ovr >= 76) pool.add(nat + " national team manager considers calling up " + name);
        if (age >= 33) pool.add("Is the end near? " + name + " battles Father Time at " + age);
        else if (age >= 30) pool.add(name + " brings veteran leadership to the " + club + " dressing room");
        if (pool.size() < 3) {
            pool.add(name + " impresses in training ahead of " + club + "'s next league fixture");
            pool.add(club + " supporters praise " + name + "'s work rate this campaign");
        }

        Collections.shuffle(pool, rnd);
        return new ArrayList<>(pool.subList(0, Math.min(3, pool.size())));
    }

    private static Map<String, String> parseQuery(String raw) {
        Map<String, String> map = new HashMap<>();
        if (raw == null || raw.isEmpty()) return map;
        for (String pair : raw.split("&")) {
            int eq = pair.indexOf('=');
            String k = eq < 0 ? pair : pair.substring(0, eq);
            String v = eq < 0 ? "" : pair.substring(eq + 1);
            map.put(URLDecoder.decode(k, StandardCharsets.UTF_8), URLDecoder.decode(v, StandardCharsets.UTF_8));
        }
        return map;
    }

    private static int num(String s, int fallback) {
        try { return s == null ? fallback : (int) Math.max(Integer.MIN_VALUE, Math.min(Integer.MAX_VALUE, Long.parseLong(s.trim()))); }
        catch (NumberFormatException e) { return fallback; }
    }

    private static String limit(String s, int max) {
        return s.length() <= max ? s : s.substring(0, max);
    }

    static String escape(String s) {
        StringBuilder sb = new StringBuilder();
        for (char c : s.toCharArray()) {
            switch (c) {
                case '"': sb.append("\""); break;
                case '\': sb.append("\\"); break;
                case '
': sb.append("\n"); break;
                case '': sb.append("\r"); break;
                case '	': sb.append("\t"); break;
                default:
                    if (c < 0x20) sb.append(String.format("\u%04x", (int) c));
                    else sb.append(c);
            }
        }
        return sb.toString();
    }

    private static void send(HttpExchange ex, int status, String body) throws IOException {
        byte[] bytes = body.getBytes(StandardCharsets.UTF_8);
        ex.getResponseHeaders().set("Content-Type", "application/json; charset=utf-8");
        ex.sendResponseHeaders(status, bytes.length);
        try (OutputStream os = ex.getResponseBody()) { os.write(bytes); }
    }
}
