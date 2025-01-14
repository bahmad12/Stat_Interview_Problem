# STAT Recovery File Processor

This console application processes ZIP files from an Amazon S3 bucket, extracts PDF files, and organizes them by PO number. The application keeps track of processed files to avoid duplicate processing.

## Features

- Downloads ZIP files from S3 bucket
- Extracts PDF files and maps them to PO numbers using CSV data
- Uploads processed PDFs to organized folders by PO number
- Maintains processing metadata to avoid duplicate processing
- Handles errors gracefully and provides detailed logging

## Prerequisites

- .NET 8.0 SDK
- AWS credentials with access to the S3 bucket

## Configuration

The application uses the following AWS credentials:
- Region: us-east-2
- Bucket: stat-coding-twvudbyqsd

AWS credentials should be configured using environment variables:
- AWS_ACCESS_KEY_ID
- AWS_SECRET_ACCESS_KEY

## Building and Running

1. Clone the repository
2. Set the AWS credentials as environment variables:
   ```powershell
   $env:AWS_ACCESS_KEY_ID="your_access_key"
   $env:AWS_SECRET_ACCESS_KEY="your_secret_key"
   ```
3. Build the project:
   ```bash
   dotnet build
   ```
4. Run the application:
   ```bash
   dotnet run
   ```

## Processing Logic

1. The application checks for ZIP files in the S3 bucket
2. For each unprocessed ZIP file:
   - Downloads and extracts the ZIP
   - Reads the CSV file to map filenames to PO numbers
   - Extracts PDF files and uploads them to `by-po/{po-number}/{original-file-name}.pdf`
   - Updates processing metadata
3. Processing metadata is stored in the S3 bucket as `processing-metadata.json` 