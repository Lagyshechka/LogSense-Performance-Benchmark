# ⚡ LogSense: High-Performance Log Analyzer

![.NET 8](https://img.shields.io/badge/.NET%208-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)
![C#](https://img.shields.io/badge/C%23-239120?style=for-the-badge&logo=c-sharp&logoColor=white)
![WPF](https://img.shields.io/badge/WPF-00599C?style=for-the-badge&logo=windows&logoColor=white)
![Status](https://img.shields.io/badge/Status-Completed-success?style=for-the-badge)

**LogSense** is a benchmark application designed to demonstrate the power of modern C# memory management techniques. It compares a **Legacy String Allocation** approach against a **High-Performance Zero-Allocation** engine using `Span<T>`, `ArrayPool`, and `Utf8Parser`.

> **Goal:** Process **500 MB+ (5 million lines)** of CSV log data with minimal RAM footprint and maximum speed.

---

##  Benchmark Results (The Proof)

The application parses a **550 MB CSV file** containing simulated server logs.

| Metric | 🐢 Legacy Mode (String.Split) | ⚡ Fast Mode (Span + ArrayPool) | Improvement |
| :--- | :--- | :--- | :--- |
| **Execution Time** | ~7.5 sec | **~0.7 sec** | **10x Faster** |
| **Peak Memory** | ~1.2 GB | **~450 MB*** | **3x Less RAM** |
| **GC Pressure** | Critical (Gen 0/1/2 spam) | **Zero Allocation** | **Optimized** |

*\*Note: The ~450MB usage in Fast Mode is strictly for storing the final `List<LogEntry>` (5M structs). The parsing process itself allocates almost **0 bytes** of temporary garbage.*

### Visual Comparison

![Slow](https://github.com/user-attachments/assets/71e004bb-c99b-41a1-b1a9-958397fe4d9d)
![Fast](https://github.com/user-attachments/assets/53cd4b11-3340-48fb-8693-097105b0187f)


--------

##  Technical Deep Dive

###  The "Slow" Approach (Legacy Mode)
The traditional way of parsing text involves `File.ReadAllLines()` and `string.Split()`.
* **Problem:** This creates millions of temporary `string` objects.
* **Result:** The Garbage Collector (GC) is overwhelmed, causing "Stop-the-World" freezes and huge memory spikes.

### The "Boosted" Approach (Fast Mode)
The optimized engine completely eliminates the need to create temporary strings during the parsing process. It achieves this through a four-step streaming pipeline:

#### 1. Buffered Streaming (IO Optimization)
Instead of loading the 500MB file into RAM, we open a `FileStream` and read the data in small chunks (e.g., 64KB). We use `ArrayPool<byte>` to rent a reusable buffer from the system. This ensures that our memory footprint remains constant, regardless of whether the file is 10MB or 10GB.

#### 2. Span<T> Slicing (Memory Safety)
Once the data is in the buffer, we use `Span<byte>` to process it. A `Span` is a lightweight pointer-like structure that allows us to "view" a section of the array without copying it. When we identify a comma or a newline, we simply create a `Slice` of the span. This operation is virtually free and allocates **zero** heap memory.

#### 3. Zero-Allocation Parsing (CPU Optimization)
Standard parsing (`int.Parse`, `DateTime.Parse`) typically requires a string input. To avoid converting our bytes into strings, we use the `Utf8Parser` static class. It takes a `ReadOnlySpan<byte>` and parses the raw UTF-8 bytes directly into primitives (`int`, `DateTime`).

#### 4. Chunk Management
If a text line is cut off at the end of a buffer, the parser intelligently copies the "leftover" bytes to the beginning of the buffer and fills the rest with the next chunk from the file stream. This ensures seamless processing of lines that cross buffer boundaries.

---

##  Tech Stack & Architecture

The project follows **Clean Architecture** principles to ensure maintainability and testability.

  * **Core:** `struct LogEntry`, `ILogParser` interface.
  * **Logic:** `FastLogParser` (Span/Memory), `LegacyLogParser` (Strings).
  * **UI (WPF):** MVVM pattern using **CommunityToolkit.Mvvm**.
  * **Dependency Injection:** `Microsoft.Extensions.DependencyInjection`.
  * **Visualization:** `LiveChartsCore.SkiaSharpView.WPF` for real-time memory graphing.

---

##  How to Run

1.  **Clone the repo:**
    ```bash
    git clone [https://github.com/Lagyshechka/LogSense-Performance-Benchmark.git](https://github.com/Lagyshechka/LogSense-Performance-Benchmark.git)
    ```
2.  **Open** `FastLogAnalyzer.sln` in Rider or Visual Studio 2022.
3.  **Generate Data:**
      * Right-click `FastLogAnalyzer.DataGenerator` -> **Run**.
      * This will create a `test_logs.csv` (~500MB) on your Desktop.
4.  **Run the App:**
      * Set `FastLogAnalyzer.UI` as startup project and Run.
      * Select the file and click **RUN BENCHMARK**.

---

