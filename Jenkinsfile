pipeline {
    agent any
    triggers {
        pollSCM('H/5 * * * *')
        githubPush()
    }

    parameters {
        string(name: 'PORT', defaultValue: '8081')
    }

    environment {
        IMAGE = 'pizza-store'
        BUILDX_NO_DEFAULT_ATTESTATIONS = '1'
    }
    
    stages {
        
        stage('Restore Dependencies') {
            steps {
                echo '=== Stage 1: Restore Dependencies ==='
                sh 'dotnet restore'
            }
        }

        stage('Build') {
            steps {
                echo '=== Stage 2: Build ==='
                sh 'dotnet build --no-restore -c Release'
            }
        }

        stage ('Quality and Testing'){
            parallel {
                stage('Tests') {
                    environment {
                        METRICS_PORT = '8082'
                    }
                    steps {
                        echo '=== Stage 3: Tests ==='
                        sh '''
                            dotnet test --no-build -c Release \
                            --logger "trx;LogFileName=test_results.trx" \
                            --collect:"XPlat Code Coverage" \
                            --results-directory ./TestResults \
                            -- DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.Format=cobertura
                        '''
                    }
                    post {
                        always {
                            mstest testResultsFile: '**/test_results.trx', keepLongStdio: true

                            recordCoverage(
                                tools: [[parser: 'COBERTURA', pattern: '**/coverage.cobertura.xml']],
                                id: 'cobertura',
                                name: 'Code Coverage',
                                sourceCodeRetention: 'LAST_BUILD'
                            )
                        }
                    }
                }

                stage('Code Style Check') {
                    steps {
                        echo '=== Checking Code Formatting ==='
                        sh 'dotnet format --verify-no-changes'
                    }
                }
            }
        }

        stage('Deploy (Publish)') {
            steps {
                echo '=== Stage 4: Deploy ==='
                sh """ 
                    docker ps -q --filter publish=${params.PORT} | xargs -r docker stop
                    docker rm pizza-store || true
                    docker build -t \$IMAGE:\$BUILD_NUMBER .
                    docker run -d --name pizza-store -p ${params.PORT}:8080 \$IMAGE:\$BUILD_NUMBER
                """
                
                echo 'The App is ready'
            }
        }
        
        stage('Check application') {
            steps {
                echo '=== Stage 5: Check ==='
                sh """
                    for i in \$(seq 1 30); do
                    if curl -fsS "http://127.0.0.1:${params.PORT}" >/dev/null; then
                        echo "App is up"
                        curl -fsS "http://127.0.0.1:${params.PORT}"
                        exit 0
                    fi
                    echo "Waiting... (\$i)"
                    sleep 2
                    done
                    echo "App did not become ready in time"
                    exit 1
                """
            }
        }

        stage('Push image') {
            steps {
                sh '''
                    set +x
                    docker login --username json_key --password-stdin cr.yandex < /var/lib/jenkins/secrets/yc-cr.json
                    CR="cr.yandex/$(cat /var/lib/jenkins/secrets/yc-registry-id)"
                    docker tag "$IMAGE:$BUILD_NUMBER" "$CR/$IMAGE:$BUILD_NUMBER"
                    docker tag "$IMAGE:$BUILD_NUMBER" "$CR/$IMAGE:latest"
                    docker push "$CR/$IMAGE:$BUILD_NUMBER"
                    docker push "$CR/$IMAGE:latest"
                '''
            }
        }

        stage('Deploy to k3s') {
            steps {
                sh '''
                    CR="cr.yandex/$(cat /var/lib/jenkins/secrets/yc-registry-id)"
                    helm upgrade --install pizza-store ./deploy/pizza-store \
                        --kubeconfig /var/lib/jenkins/.kube/config \
                        --set image.repository="$CR/$IMAGE" \
                        --set-string image.tag="$BUILD_NUMBER" \
                        --wait \
                        --timeout 5m
                '''
            }
        }
    }

    post {
        always {
            echo 'Cleaning...'
            script {
                try {
                    mail to: env.MAIL_TO,
                         from: env.MAIL_FROM,
                         subject: "pizza-app #${env.BUILD_NUMBER}: ${currentBuild.currentResult}",
                         body: """pizza-app #${env.BUILD_NUMBER}: ${currentBuild.currentResult}
${currentBuild.absoluteUrl}"""
                } catch (err) {
                    echo "Email notification failed: ${err.message}"
                }
            }
        }
        success {
            echo 'Pipeline finished successfully'
        }
        failure {
            echo 'The build failed!'
        }
        unstable {
            echo 'The build is unstable!'
        }
    }
}
