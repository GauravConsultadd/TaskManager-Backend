pipeline {
    agent any
    
    stages {
        stage('Build') {
            steps {
                script {
                    // Clean and build the ASP.NET Core application
                    bat 'dotnet build TaskManager.csproj --configuration Release'
                }
            }
        }
        
        stage('Test') {
            steps {
                script {
                    // Run tests if needed
                    bat 'dotnet test TaskManager.Tests.csproj --configuration Release'
                }
            }
        }
        
        stage('Publish') {
            steps {
                script {
                    // Publish the ASP.NET Core application
                    bat 'dotnet publish TaskManager.csproj --configuration Release --output publish_output'
                }
            }
        }
        
        stage('Deploy') {
            steps {
                // You can deploy to your desired environment here
                // For example, copy files to a server, deploy to Azure, etc.
                // This step depends on your deployment strategy.
            }
        }
    }
}
